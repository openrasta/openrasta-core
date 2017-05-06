using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.PreExecuteContributor
{
  public class contributor_RenderNow : middleware_context
  {
    [Fact]
    public async Task pipeline_continues()
    {

      var middleware = new PreExecuteMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.RenderNow)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.RenderNow);
    }
  }
}