using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public class pipeline_throws_once : response_retry
  {

    public pipeline_throws_once()
    {
      next_is(async env =>
      {
        if (NextCallCount == 0) throw new PipelineAbortedException();
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;
      });
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
    public void no_exception_thrown()
    {
      Exception.ShouldBeNull();
    }

    [Fact]
    public void original_exception_is_recorded()
    {
      Env.ServerErrors.Single().Exception.ShouldBeOfType<PipelineAbortedException>();
    }
  }
}