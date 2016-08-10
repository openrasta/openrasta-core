using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public IEnumerable<ContributorCall> CallGraph { get; }

    public DoubleTapPipelineAdaptor(IEnumerable<IPipelineContributor> contributors, IGenerateCallGraphs graphs)
    {
      _graphs = graphs;
      Contributors = contributors.ToList().AsReadOnly();
    }
    public void Initialize()
    {
      _invoker = DoubleTapPipelineBuilder.Build(_graphs, Contributors).Invoke;
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