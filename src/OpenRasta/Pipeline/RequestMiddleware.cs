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

    public override Task Invoke(ICommunicationContext env)
    {
      if (env.PipelineData.TryGetValue("skipToCleanup",out var isSkip) && isSkip is bool skip && skip)
      {
        return Next.Invoke(env);
      }

      return env.PipelineData.PipelineStage.CurrentState != PipelineContinuation.Continue 
        ? Next.Invoke(env)
        : InvokeContributor(env);
    }

     async Task InvokeContributor(ICommunicationContext env)
    {
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