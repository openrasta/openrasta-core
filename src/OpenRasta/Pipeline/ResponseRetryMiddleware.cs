using System;
using System.Threading.Tasks;
using OpenRasta.Web;
using OpenRasta.Web.Internal;

namespace OpenRasta.Pipeline
{
  public class ResponseRetryMiddleware : IPipelineMiddlewareFactory, IPipelineMiddleware
  {
    IPipelineMiddleware _responsePipeline;

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      _responsePipeline = next;
      return this;
    }

    public Task Invoke(ICommunicationContext env)
    {
      return LoggingInvoke(env);
    }

    async Task LoggingInvoke(ICommunicationContext env)

    {
      var shouldRetry = await ShouldRetryRendering(env);

      if (!shouldRetry) return;
      
      EnsureCanRetry(env);

      env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;
      env.PipelineData.ResponseCodec = null;


      try
      {
        await _responsePipeline.Invoke(env);
      }
      catch (Exception e)
      {
        ThrowWithNewError(env, "Error re-rendering the response",
          "An error occured while trying to re-rendering the response.", e);
      }

      if (env.PipelineData.PipelineStage.CurrentState == PipelineContinuation.RenderNow)
        ThrowWithNewError(env,
          "Re-rendering failed",
          "A component set the response pipeline in render now mode one too many times.");
    }

    static void ThrowWithNewError(ICommunicationContext env, string title, string message, Exception e = null)
    {
      env.ServerErrors.Add(new Error
      {
        Title = title,
        Message = message,
        Exception = e
      });

#pragma warning disable 618
      // set for compatibility
      env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
#pragma warning restore 618

      throw new PipelineAbortedException(env.ServerErrors);
    }

    static void EnsureCanRetry(ICommunicationContext env)
    {
      if (env.Response.HeadersSent)
        ThrowWithNewError(env,
          "Response could not be retried",
          "A component in the response pipeline tried to render new content, but HTTP headers were already sent. Try enabling buffering for the current codec.");
    }

    async Task<bool> ShouldRetryRendering(ICommunicationContext env)
    {
      try
      {
        await _responsePipeline.Invoke(env);

        if (env.PipelineData.PipelineStage.CurrentState != PipelineContinuation.RenderNow)
          return false;
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error {Exception = e});
        env.SetOperationResultToServerErrors();
      }

      return true;
    }
  }
}