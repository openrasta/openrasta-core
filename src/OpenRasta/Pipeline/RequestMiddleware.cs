using System;
using System.Threading.Tasks;
using OpenRasta.Web;
using OpenRasta.Web.Internal;

namespace OpenRasta.Pipeline
{
  public class RequestMiddleware : AbstractContributorMiddleware
  {
    readonly bool _catchExceptions;

    public RequestMiddleware(ContributorCall call, bool catchExceptions = true)
      : base(call)
    {
      _catchExceptions = catchExceptions;
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      if (env.PipelineData.TryGetValue("skipToCleanup",out var isSkip) && isSkip is bool skip && skip)
      {
        await Next.Invoke(env);
        return;
      }
      
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
      catch (Exception e) when (_catchExceptions)
      {
        env.Abort(e);
      }
      catch (Exception)
      {
        env.PipelineData["skipToCleanup"] = true;
        await Next.Invoke(env);
        throw;
      }

      await Next.Invoke(env);
    }
  }
}