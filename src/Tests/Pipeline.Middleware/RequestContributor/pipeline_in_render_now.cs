using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.RequestContributor
{
  public class pipeline_in_render_now : middleware_context
  {
    [Fact]
    public async Task contributor_not_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.RenderNow;

      var middleware = new RequestMiddleware(
        Contributor(e => Task.FromResult(PipelineContinuation.Continue)))
        .Compose(Next);
      await middleware.Invoke(Env);

      NextCalled.ShouldBeTrue();
      ContributorCalled.ShouldBeFalse();
    }
  }
}