using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.request
{
  public class pipeline_in_continue_contrib_aborts : middleware_context
  {
    [Fact]
    public async Task pipeline_is_Abort()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;
      var middleware = new RequestMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Abort)));

      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Abort);
    }
  }
}