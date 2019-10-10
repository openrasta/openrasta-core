using OpenRasta.Pipeline;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  public class PostExecuteMarkerContributor : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(context =>
      {
        context.PipelineData[nameof(PostExecuteMarkerContributor)] = true;
        return PipelineContinuation.Continue;
      }).After<KnownStages.IEnd>();
    }
  }
}