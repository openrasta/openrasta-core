using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_RenderNow_then_throws : response_retry
  {
    public pipeline_RenderNow_then_throws()
    {
      next_is(async env =>
      {
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.RenderNow;
        if (NextCallCount == 1) throw new PipelineAbortedException();
      });
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_reexecuted()
    {
      NextCallCount.ShouldBe(2);
    }

    [Fact]
    public void  pipeline_throws()
    {
      Exception.ShouldBeOfType<PipelineAbortedException>();
    }
  }
}