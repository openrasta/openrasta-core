using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class PipelineAsync : IPipelineAsync
  {
    readonly IPipelineMiddleware _middleware;
    public IEnumerable<IPipelineContributor> Contributors { get; }
    public IEnumerable<ContributorCall> CallGraph { get; set; }
    public IEnumerable<IPipelineMiddlewareFactory> MiddlewareFactories { get; }

    public
      PipelineAsync(IPipelineMiddleware middleware, IEnumerable<IPipelineContributor> contributors,
        IEnumerable<ContributorCall> callGraph, List<IPipelineMiddlewareFactory> middlewareFactories)
    {
      _middleware = middleware;
      Contributors = contributors.ToList().AsReadOnly();
      CallGraph = callGraph;
      MiddlewareFactories = middlewareFactories;
    }

    public Task RunAsync(ICommunicationContext env)
    {
      if (env.PipelineData.PipelineStage == null)
        env.PipelineData.PipelineStage = new PipelineStage(CallGraph);
      return _middleware.Invoke(env);
    }
  }
}