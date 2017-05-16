using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseContributor
{
  public class pipeline_in_continue_contrib_RenderNow : middleware_context
  {
    [Fact]
    public async Task next_middleware_isnt_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;

      var middleware = new ResponseMiddleware(
        Contributor(e => Task.FromResult(PipelineContinuation.RenderNow)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      NextCalled.ShouldBeFalse();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.RenderNow);
    }
  }
}