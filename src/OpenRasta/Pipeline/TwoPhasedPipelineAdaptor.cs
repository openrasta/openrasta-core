using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class TwoPhasedPipelineAdaptor : IPipeline, IPipelineAsync
  {
    readonly IGenerateCallGraphs _callGrapher;
    Func<ICommunicationContext, Task> _invoker;
    public bool IsInitialized { get; private set; }
    public IList<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; private set; }
    public StartupProperties StartupProperties { get; private set; }

    public TwoPhasedPipelineAdaptor(IDependencyResolver resolver)
    {
      _callGrapher = resolver.HasDependency<IGenerateCallGraphs>()
        ? resolver.Resolve<IGenerateCallGraphs>()
        : new WeightedCallGraphGenerator();
      Contributors = resolver.ResolveAll<IPipelineContributor>()
        .ToList()
        .AsReadOnly();
    }

    public void Initialize()
    {
      Initialize(new StartupProperties());
    }

    public void Initialize(StartupProperties startup)
    {
      if (startup.OpenRasta.Pipeline.Validate)
        Contributors.VerifyKnownStagesRegistered();

      _invoker = (CallGraph = _callGrapher.GenerateCallGraph(Contributors))
        .ToTwoPhasedMiddleware<KnownStages.IOperationResultInvocation>()
        .Invoke;
      IsInitialized = true;
    }


    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> notification)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    [Obsolete("Don't do it this will deadlock.")]
    public void Run(ICommunicationContext context)
    {
      RunAsync(context).GetAwaiter().GetResult();
    }

    public Task RunAsync(ICommunicationContext env)
    {
      this.CheckPipelineInitialized();

      if (env.PipelineData.PipelineStage == null)
        env.PipelineData.PipelineStage = new PipelineStage(((IPipeline) this).CallGraph);
      return _invoker(env);
    }

    public IPipelineExecutionOrder Use(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }
  }
}