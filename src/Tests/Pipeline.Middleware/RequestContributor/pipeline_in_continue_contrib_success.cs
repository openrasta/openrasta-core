using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.RequestContributor
{
  public class pipeline_in_continue_contrib_success : middleware_context
  {
    [Fact]
    public async Task contributor_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;

      var middleware = new RequestMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Continue);
    }
  }
}