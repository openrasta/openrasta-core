using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.PreExecuteContributor
{
  public class contributor_continues : middleware_context
  {
    [Fact]
    public async Task middleware_throws()
    {
      var middleware = new PreExecuteMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Continue);
    }
  }
}