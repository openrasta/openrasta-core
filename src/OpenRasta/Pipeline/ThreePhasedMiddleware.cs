using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ThreePhasedMiddleware : IPipelineMiddleware
  {
    readonly IPipelineMiddleware _preRequestPipeline;
    readonly IPipelineMiddleware _requestPipeline;
    readonly IPipelineMiddleware _responsePipeline;
    readonly IPipelineMiddleware _catastrophicFail;
    readonly IPipelineMiddleware _cleanup;

    public ThreePhasedMiddleware(
      IPipelineMiddleware preRequestPipeline,
      IPipelineMiddleware requestPipeline,
      IPipelineMiddleware responsePipeline,
      IPipelineMiddleware catastrophicFail,
      IPipelineMiddleware cleanup)
    {
      _preRequestPipeline = preRequestPipeline;
      _requestPipeline = requestPipeline;
      _responsePipeline = responsePipeline;
      _catastrophicFail = catastrophicFail;
      _cleanup = cleanup;
    }
    public async Task Invoke(ICommunicationContext env)
    {
      try
      {
        await _preRequestPipeline.Invoke(env);
        await InvokeSafe(_requestPipeline, env);
        await _responsePipeline.Invoke(env);
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error { Exception = e});
        await _catastrophicFail.Invoke(env);
      }
      finally
      {
        await _cleanup.Invoke(env);
      }
    }

    static async Task InvokeSafe(IPipelineMiddleware middleware, ICommunicationContext env)
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
