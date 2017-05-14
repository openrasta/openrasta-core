namespace OpenRasta.Pipeline
{
  class NullPipelineContributor<TAfter, TBefore> : IPipelineContributor
    where TBefore : IPipelineContributor where TAfter : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner
        .Notify(_ => PipelineContinuation.Continue)
        .Before<TBefore>().And
        .After<TAfter>();
    }
  }
}