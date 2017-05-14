namespace OpenRasta.Pipeline
{
  class NullOrderedPipelineContributor<TAfter, TBefore> : IPipelineContributor
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