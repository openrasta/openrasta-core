using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.RequestContributor
{
  public class pipeline_in_abort : middleware_context
  {
    [Fact]
    public async Task contributor_not_executed()
    {
      Env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;

      var middleware = new RequestMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Continue)));
      await middleware.Invoke(Env);

      ContributorCalled.ShouldBeFalse();
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Abort);

    }
  }
}