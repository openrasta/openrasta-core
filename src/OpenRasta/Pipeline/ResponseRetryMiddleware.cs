using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ResponseRetryMiddleware : IPipelineMiddlewareFactory, IPipelineMiddleware
  {
    IPipelineMiddleware _responsePipeline;
    ILogger log = NullLogger.Instance;

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      _responsePipeline = next;
      return this;
    }

    async Task LoggingInvoke(ICommunicationContext env)
    {
      try
      {
        await InvokeWithErrorConneg(env);
      }
      finally
      {
        foreach (var error in env.ServerErrors)
        {
          log.WriteInfo(error.Exception.ToString());
        }
      }
    }

    public Task Invoke(ICommunicationContext env)
    {
      return LoggingInvoke(env);
    }

    async Task InvokeWithErrorConneg(ICommunicationContext env)
    {
      var exceptionHappened = false;

      try
      {
        await _responsePipeline.Invoke(env);
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error {Exception = e});
        exceptionHappened = true;
      }
      if (exceptionHappened)
      {
        env.OperationResult = OperationResultForExceptions(env);
      }
      if (exceptionHappened
          || env.PipelineData.PipelineStage.CurrentState == PipelineContinuation.RenderNow)
      {
        env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Continue;
        env.PipelineData.ResponseCodec = null;

        try
        {
          await _responsePipeline.Invoke(env);
          if (env.PipelineData.PipelineStage.CurrentState == PipelineContinuation.RenderNow)
            throw new PipelineAbortedException(env.ServerErrors);
        }
        catch (Exception e)
        {
          env.ServerErrors.Add(new Error {Exception = e});
#pragma warning disable 618 - Compatibility
          env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;
#pragma warning restore 618

          throw new PipelineAbortedException(env.ServerErrors);
        }
      }
    }

    static OperationResult.InternalServerError OperationResultForExceptions(ICommunicationContext env)
    {
      return new OperationResult.InternalServerError()
      {
        Title = "Errors happened while executing the request",
        ResponseResource = env.ServerErrors.ToList(),
        Description = $"Errors happened while executing the request: {Environment.NewLine}" +
                      string.Concat(
                        env.ServerErrors.Select(
                          error => error.ToString() + Environment.NewLine))
      };
    }
  }
}