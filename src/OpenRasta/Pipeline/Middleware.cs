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

    static readonly Task<PipelineContinuation> IdentitySingleTapResult = Task.FromResult(PipelineContinuation.Continue);

    public static Task<PipelineContinuation> IdentitySingleTap(ICommunicationContext context) =>
      IdentitySingleTapResult;

    class NothingPipelineMiddleware : IPipelineMiddleware
    {
      Task IPipelineMiddleware.Invoke(ICommunicationContext context)
      {
        return Task.FromResult(true);
      }
    }

    public static IPipelineMiddleware Compose(this IEnumerable<IPipelineMiddlewareFactory> components)
    {
      return components.Reverse().Aggregate(Identity, (next, factory) => factory.Compose(next));
    }
  }
}
