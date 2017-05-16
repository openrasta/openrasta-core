using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_in_Abort : response_retry
  {
    public pipeline_in_Abort()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
      next_sets_pipeline_to(PipelineContinuation.Continue);
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_set_to_Continue()
    {
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Continue);
    }
  }
}