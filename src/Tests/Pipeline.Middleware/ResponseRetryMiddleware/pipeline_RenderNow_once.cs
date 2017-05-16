using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_RenderNow_once : response_retry
  {
    public pipeline_RenderNow_once()
    {
      next_sets_pipeline_to(PipelineContinuation.RenderNow, PipelineContinuation.Continue);
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_reexecuted()
    {
      NextCallCount.ShouldBe(2);
    }

    [Fact]
    public void pipeline_is_set_to_continue()
    {
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Continue);
    }
    [Fact]
    public void no_errors()
    {
      Exception.ShouldBeNull();
    }
  }
}