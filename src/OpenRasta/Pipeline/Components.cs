using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public static class Components
  {
      public static readonly IPipelineComponent Identity = new NothingPipelineComponent();
      public static Task<PipelineContinuation> IdentitySingleTap(ICommunicationContext context) => Task.FromResult(PipelineContinuation.Continue);
    class NothingPipelineComponent : IPipelineComponent
    {
      Task IPipelineComponent.Invoke(ICommunicationContext context)
      {
        return Task.FromResult(0);
      }
    }

    public static IPipelineComponent BuildPipeline(this IEnumerable<IPipelineMiddleware> components)
    {
      return components.Aggregate(Identity, (next, previous) => previous.Build(next));
    }
  }
}