using System;
using OpenRasta.Diagnostics;
using OpenRasta.Web;
using OpenRasta.Web.Internal;

namespace OpenRasta.Pipeline.Contributors
{
  public class ResourceTypeResolverContributor : KnownStages.IUriMatching
  {
    readonly IUriResolver _uriRepository;

    public ResourceTypeResolverContributor(IUriResolver uriRepository)
    {
      _uriRepository = uriRepository;
      Log = NullLogger.Instance;
    }

    public ILogger Log { get; set; }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ResolveResource);
    }

    PipelineContinuation ResolveResource(ICommunicationContext context)
    {
      if (context.PipelineData.SelectedResource == null)
      {
        var uriToMath = context.GetRequestUriRelativeToRoot();
        var uriMatch = _uriRepository.Match(uriToMath);
        if (uriMatch != null)
        {
          context.PipelineData.SelectedResource = uriMatch;
          context.PipelineData.ResourceKey = uriMatch.ResourceKey;
          context.Request.UriName = uriMatch.UriName;
        }
        else
        {
          context.OperationResult = CreateNotFound(context);
          return PipelineContinuation.RenderNow;
        }
      }
      else
      {
        Log.WriteInfo(
            $"Not resolving any resource as a resource with key {context.PipelineData.SelectedResource.ResourceKey} has already been selected.");
      }

      return PipelineContinuation.Continue;
    }

    OperationResult.NotFound CreateNotFound(ICommunicationContext context)
    {
      return new OperationResult.NotFound
      {
          Description =
              "No registered resource could be found for "
              + context.Request.Uri,
          Reason = NotFoundReason.NotMapped
      };
    }
  }
}
