using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseContributor
{
  public class pipeline_in_abort_contrib_success : middleware_context
  {
    [Fact]
    public async Task contributor_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;

      var middleware = new ResponseMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeTrue();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Abort);

    }
  }
}