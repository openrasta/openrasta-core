using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_RenderNow_twice : response_retry
  {
    public pipeline_RenderNow_twice()
    {
      next_sets_pipeline_to(PipelineContinuation.RenderNow, PipelineContinuation.RenderNow);
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_reexecuted()
    {
      NextCallCount.ShouldBe(2);
    }

    [Fact]
    public void pipeline_throws()
    {
      Exception.ShouldBeOfType<PipelineAbortedException>();
    }
  }
}