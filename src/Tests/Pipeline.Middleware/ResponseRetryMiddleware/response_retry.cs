using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware.ResponseRetryMiddleware
{
  public abstract class response_retry
  {
    public response_retry()
    {
      Env = new InMemoryCommunicationContext();
      Middleware = new OpenRasta.Pipeline.ResponseRetryMiddleware();
    }

    public OpenRasta.Pipeline.ResponseRetryMiddleware Middleware { get; set; }

    protected IPipelineMiddleware Next;
    protected int NextCallCount;

    public InMemoryCommunicationContext Env { get; set; }
    public Exception Exception { get; set; }


    protected void next_is(Func<ICommunicationContext, Task> invoke)
    {
      Next = new DelegateMiddleware(env =>
      {
        try
        {
          return invoke(env);
        }
        finally
        {
          NextCallCount++;
        }
      });
    }

    protected void next_sets_pipeline_to(params PipelineContinuation[] states)
    {
      Next = new DelegateMiddleware(async env =>
      {
        env.PipelineData.PipelineStage.CurrentState = states[NextCallCount];
        NextCallCount++;
      });
    }

    protected void InvokePipeline()
    {
      try
      {
        Middleware.Compose(Next).Invoke(Env).GetAwaiter().GetResult();
      }
      catch (Exception e)
      {
        Exception = e;
      }
    }
  }
}