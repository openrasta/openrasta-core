using System;
using System.Threading.Tasks;
using OpenRasta.Web;
using OpenRasta.Web.Internal;

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
      {
        await Next.Invoke(env);
        return;
      }

      try
      {
        currentState
          = env.PipelineData.PipelineStage.CurrentState
            = await Contributor(env);
      }
      catch (Exception e)
      {
        env.Abort(e);
      }
      await Next.Invoke(env);
    }
  }
}


