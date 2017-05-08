using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Examples
{
  class UriMatchingContributor : KnownStages.IUriMatching
  {
    public void Initialize(IPipeline pipelineRunner)
    {
    }
  }
}