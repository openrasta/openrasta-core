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
  public class ThreePhasedPipelineAdaptor : IPipeline, IPipelineAsync, IPipelineInitializer
  {
    readonly IGenerateCallGraphs _callGrapher;
    Func<ICommunicationContext, Task> _invoker;
    public bool IsInitialized { get; private set; }
    public IList<IPipelineContributor> Contributors { get; private set; }
    public IEnumerable<ContributorCall> CallGraph { get; private set; }

    public ThreePhasedPipelineAdaptor(IDependencyResolver resolver)
    {
      _callGrapher =new CallGraphGeneratorFactory(resolver)
        .GetCallGraphGenerator();
      Contributors = resolver.ResolveAll<IPipelineContributor>()
        .ToList()
        .AsReadOnly();
    }

    public void Initialize()
    {
      Initialize(new StartupProperties());
    }

    public IPipelineAsync Initialize(StartupProperties startup)
    {
      if (startup.OpenRasta.Pipeline.Validate)
        Contributors.VerifyKnownStagesRegistered();

      var defaults = new List<IPipelineMiddlewareFactory>();
      if (startup.OpenRasta.Errors.HandleCatastrophicExceptions)
      {
        defaults.Add(new CatastrophicFailureMiddleware());
      }
      CallGraph = _callGrapher.GenerateCallGraph(Contributors).ToList();
      var contributorMiddleware = CallGraph.ToMiddleware(startup.OpenRasta.Pipeline.ContributorTrailers);
      Contributors = CallGraph.Select(call => call.Target).ToList().AsReadOnly();

      _invoker =
        defaults.Concat(contributorMiddleware)
        .Compose()
        .Invoke;
      IsInitialized = true;
      return this;
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

    IEnumerable<IPipelineContributor> IPipelineAsync.Contributors => Contributors;

    public Task RunAsync(ICommunicationContext env)
    {
      this.CheckPipelineInitialized();

      if (env.PipelineData.PipelineStage == null)
        env.PipelineData.PipelineStage = new PipelineStage(((IPipeline) this).CallGraph);
      return _invoker(env);
    }

    public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }
  }
}