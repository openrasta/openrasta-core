using OpenRasta.Pipeline;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  class ContributorExecutingAfter<T> : KnownStages.IOperationResultInvocation where T : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(env => PipelineContinuation.Continue).After<T>();
    }
  }
}