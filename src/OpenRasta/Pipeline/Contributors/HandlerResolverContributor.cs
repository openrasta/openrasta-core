using System.Linq;
using OpenRasta.Handlers;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  /// <summary>
  /// Resolves the handler attached to a resource type.
  /// </summary>
  public class HandlerResolverContributor : KnownStages.IHandlerSelection
  {
    readonly IHandlerRepository _handlers;

    public HandlerResolverContributor(IHandlerRepository repository)
    {
      _handlers = repository;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ResolveHandler).After<KnownStages.IUriMatching>();
    }

    PipelineContinuation ResolveHandler(ICommunicationContext context)
    {
      var handlerTypes = _handlers.GetHandlerTypesFor(context.PipelineData.ResourceKey);

      var enumerable = handlerTypes as IType[] ?? handlerTypes.ToArray();
      if (handlerTypes == null || !enumerable.Any()) return PipelineContinuation.Abort;
      context.PipelineData.SelectedHandlers = enumerable.ToList();
      return PipelineContinuation.Continue;
    }
  }
}