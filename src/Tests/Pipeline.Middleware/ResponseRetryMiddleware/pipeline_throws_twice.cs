using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_throws_twice : response_retry
  {

    public pipeline_throws_twice()
    {
      next_is(env => throw new PipelineAbortedException());
      InvokePipeline();
    }

    [Fact]
    public void pipeline_is_reexecuted()
    {
      NextCallCount.ShouldBe(2);
    }

    [Fact]
    public void pipeline_is_set_to_abort()
    {
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Abort);
    }
    [Fact]
    public void exception_is_thrown()
    {
      Exception.ShouldBeOfType<PipelineAbortedException>();
    }

    [Fact]
    public void original_exception_is_recorded()
    {
      Env.ServerErrors.Count.ShouldBe(2);
      Env.ServerErrors.ShouldAllBe(
        error => error.Exception is PipelineAbortedException);
    }
  }
}