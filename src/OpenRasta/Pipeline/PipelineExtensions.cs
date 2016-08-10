using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public static class PipelineExtensions
  {
    static readonly IPipelineComponent Nothing = new NothingPipelineComponent();

    class NothingPipelineComponent : IPipelineComponent
    {
      Task IPipelineComponent.Invoke(ICommunicationContext context)
      {
        return Task.FromResult(0);
      }
    }

    public static IPipelineComponent BuildPipeline(this IEnumerable<IPipelineMiddleware> components)
    {
      return components.Aggregate(Nothing, (next, previous) => previous.Build(next));
    }
  }
}