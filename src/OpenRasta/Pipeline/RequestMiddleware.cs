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
          = await InvokeSingleTap(env);

      if (currentState == PipelineContinuation.Abort)
        throw new PipelineAbortedException();

      await Next.Invoke(env);
    }
  }

  public class PipelineAbortedException : Exception
  {
  }
}


