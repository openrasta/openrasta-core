using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class PipelineAsync : IPipelineAsync
  {
    readonly IPipelineMiddleware _middleware;
    public IEnumerable<IPipelineContributor> Contributors { get; }
    IEnumerable<ContributorCall> CallGraph { get; set; }

    public PipelineAsync(
      IEnumerable<IPipelineContributor> contributors,
      IPipelineMiddleware middleware,
      IEnumerable<ContributorCall> callGraph)
    {
      _middleware = middleware;
      Contributors = contributors.ToList().AsReadOnly();
      CallGraph = callGraph;
    }

    public Task RunAsync(ICommunicationContext env)
    {
      if (env.PipelineData.PipelineStage == null)
        env.PipelineData.PipelineStage = new PipelineStage(CallGraph);
      return _middleware.Invoke(env);
    }
  }
}