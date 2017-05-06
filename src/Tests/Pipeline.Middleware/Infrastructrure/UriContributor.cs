using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Infrastructrure
{
  class UriContributor : KnownStages.IUriMatching
  {
    public void Initialize(IPipeline pipelineRunner) {}
  }
}