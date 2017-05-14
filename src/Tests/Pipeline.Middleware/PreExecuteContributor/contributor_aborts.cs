using System.Threading.Tasks;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.PreExecuteContributor
{
  public class contributor_aborts : middleware_context
  {
    [Fact]
    public void middleware_throws()
    {
      var middleware = new PreExecuteMiddleware(Contributor(e => Task.FromResult(PipelineContinuation.Abort)))
        .Compose(Next);
      middleware.Invoke(Env).ShouldThrow<PipelineAbortedException>();

      ContributorCalled.ShouldBeTrue();
      NextCalled.ShouldBeFalse();

#pragma warning disable 618
      Env.PipelineData.PipelineStage.CurrentState.ShouldBe(PipelineContinuation.Abort);
#pragma warning restore 618
    }
  }
}