using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Examples
{
  class OperationResultContributor : KnownStages.IOperationResultInvocation
  {
    public void Initialize(IPipeline pipelineRunner) {}
  }
}