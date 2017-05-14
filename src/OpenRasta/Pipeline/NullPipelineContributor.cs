namespace OpenRasta.Pipeline
{
  public class NullPipelineContributor : IPipelineContributor
  {
    NullPipelineContributor() {}
    public static NullPipelineContributor Instance { get; } = new NullPipelineContributor();
    void IPipelineContributor.Initialize(IPipeline pipelineRunner) {}
  }
}