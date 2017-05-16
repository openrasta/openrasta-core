using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Web;

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

    public async Task Invoke(ICommunicationContext env)
    {
      var exceptionHappened = false;

      try
      {
        await _responsePipeline.Invoke(env);
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error { Exception =  e});
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
        try
        {
          await _responsePipeline.Invoke(env);
          if (env.PipelineData.PipelineStage.CurrentState == PipelineContinuation.RenderNow)
            throw new PipelineAbortedException();
        }
        catch (Exception e)
        {
          env.ServerErrors.Add(new Error { Exception =  e});
          env.PipelineData.PipelineStage.CurrentState = PipelineContinuation.Abort;

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
                          error=>error.ToString()+Environment.NewLine))
      };
    }
  }
}