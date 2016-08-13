using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public static class Middleware
  {
    public static IPipelineMiddleware Identity { get; } = new NothingPipelineMiddleware();

    public static Task<PipelineContinuation> IdentitySingleTap(ICommunicationContext context)
      => Task.FromResult(PipelineContinuation.Continue);

    class NothingPipelineMiddleware : IPipelineMiddleware
    {
      Task IPipelineMiddleware.Invoke(ICommunicationContext context)
      {
        return Task.FromResult(0);
      }
    }

    public static IPipelineMiddleware BuildPipeline(this IEnumerable<IPipelineMiddlewareFactory> components)
    {
      return components.Aggregate(Identity, (next, factory) => factory.Build(next));
    }
  }
}
