using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseContributor
{
  public class pipeline_in_render_now : middleware_context
  {
    [Fact]
    public async Task contributor_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.RenderNow;

      var middleware = new ResponseMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.RenderNow;
    }
  }
}