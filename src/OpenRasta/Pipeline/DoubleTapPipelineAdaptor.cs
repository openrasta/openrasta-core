using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class DoubleTapPipelineAdaptor : IPipeline, IPipelineAsync
  {
    readonly IGenerateCallGraphs _graphs;
    Func<ICommunicationContext,Task> _invoker;
    public bool IsInitialized { get; }
    public IList<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; private set; }

    public DoubleTapPipelineAdaptor(IDependencyResolver resolver)
    {
      _graphs = resolver.HasDependency<IGenerateCallGraphs>()
          ? resolver.Resolve<IGenerateCallGraphs>()
          : new WeightedCallGraphGenerator();
      Contributors = resolver.ResolveAll<IPipelineContributor>().ToList().AsReadOnly();
    }
    public void Initialize()
    {
      IEnumerable<IPipelineContributor> contributors = Contributors;
      _invoker = DoubleTapPipelineBuilder.Build(CallGraph = _graphs.GenerateCallGraph(contributors)).Invoke;
    }

    public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> notification)
    {
      throw new NotImplementedException("Should never be called here, ever!");
    }

    public void Run(ICommunicationContext context)
    {
      RunAsync(context).GetAwaiter().GetResult();
    }

    public Task RunAsync(ICommunicationContext env)
    {
      return _invoker(env);
    }
  }
}
