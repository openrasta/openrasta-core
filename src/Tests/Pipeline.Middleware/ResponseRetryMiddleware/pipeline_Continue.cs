using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_Continue : response_retry
  {
    public pipeline_Continue()
    {
      next_sets_pipeline_to(PipelineContinuation.Continue);
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_not_reexecuted()
    {
      NextCallCount.ShouldBe(1);
    }

    [Fact]
    public void no_errors()
    {
      Exception.ShouldBeNull();
    }
  }
}