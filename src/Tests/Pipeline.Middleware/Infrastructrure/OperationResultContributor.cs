using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Infrastructrure
{
  class OperationResultContributor : KnownStages.IOperationResultInvocation
  {
    public void Initialize(IPipeline pipelineRunner) {}
  }
}