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
    public async Task middleware_continues()
    {
      var middleware = new PreExecuteMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)))
        .Compose(Next);
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      NextCalled.ShouldBeTrue();

      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Continue);
    }
  }
}