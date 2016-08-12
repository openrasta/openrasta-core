using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class DoubleTapMiddleware : IPipelineMiddleware
  {
    readonly IPipelineMiddleware _requestPipeline;
    readonly IPipelineMiddleware _responsePipeline;
    readonly IPipelineMiddleware _catastrophicFail;
    readonly IPipelineMiddleware _cleanup;

    public DoubleTapMiddleware(
      IPipelineMiddleware requestPipeline,
      IPipelineMiddleware responsePipeline,
      IPipelineMiddleware catastrophicFail,
      IPipelineMiddleware cleanup)
    {
      _requestPipeline = requestPipeline;
      _responsePipeline = responsePipeline;
      _catastrophicFail = catastrophicFail;
      _cleanup = cleanup;
    }
    public async Task Invoke(ICommunicationContext env)
    {
      await InvokeSafe(_requestPipeline,env);
      await InvokeSafe(_responsePipeline, env);
      if (env.PipelineData.PipelineStage.CurrentState == PipelineContinuation.Abort)
        await _catastrophicFail.Invoke(env);
      await _cleanup.Invoke(env);
    }

    async Task InvokeSafe(IPipelineMiddleware middleware, ICommunicationContext env)
    {
      try
      {
        await middleware.Invoke(env);
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error {Exception = e});
        env.Abort();
      }
    }
  }
}
