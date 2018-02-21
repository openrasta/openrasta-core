namespace OpenRasta.Pipeline
{
  class NullAuthenticationContributor : KnownStages.IAuthentication
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(context => PipelineContinuation.Continue).After<KnownStages.IBegin>();
    }
  }
}