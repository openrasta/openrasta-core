using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class RequestMiddleware : AbstractContributorMiddleware
  {
    public RequestMiddleware(
      Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
      : base(singleTapContributor)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      var currentState = env.PipelineData.PipelineStage.CurrentState;

      if (currentState != PipelineContinuation.Continue)
        return;

      currentState
        = env.PipelineData.PipelineStage.CurrentState
          = await Contributor(env);

#pragma warning disable 618
      if (currentState == PipelineContinuation.Abort)
#pragma warning restore 618
        throw new PipelineAbortedException();

      await Next.Invoke(env);
    }
  }
}


