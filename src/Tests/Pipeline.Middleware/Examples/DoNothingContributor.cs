using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Examples
{
  class DoNothingContributor : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
    }
  }
}