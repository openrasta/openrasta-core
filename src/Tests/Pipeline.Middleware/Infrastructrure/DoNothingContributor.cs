using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Infrastructrure
{
  class DoNothingContributor : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
    }
  }
}