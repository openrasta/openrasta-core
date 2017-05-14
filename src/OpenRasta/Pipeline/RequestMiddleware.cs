using System;
using System.Threading.Tasks;
using OpenRasta.Web;
using OpenRasta.Web.Internal;

namespace OpenRasta.Pipeline
{
  public class RequestMiddleware : AbstractContributorMiddleware
  {
    public RequestMiddleware(ContributorCall call)
      : base(call)
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
        env.PipelineData.PipelineStage.CurrentState
            = await ContributorInvoke(env);
      }
      catch (Exception e)
      {
        env.Abort(e);
      }
      await Next.Invoke(env);
    }
  }
}


