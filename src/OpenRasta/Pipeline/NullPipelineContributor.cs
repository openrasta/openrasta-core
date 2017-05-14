namespace OpenRasta.Pipeline
{
  public class NullPipelineContributor : IPipelineContributor
  {
    NullPipelineContributor() {}
    public static NullPipelineContributor Instance { get; } = new NullPipelineContributor();
    public void Initialize(IPipeline pipelineRunner) {}
  }
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