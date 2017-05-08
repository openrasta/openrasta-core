using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Pipeline.Middleware.Infrastructrure
{
  public abstract class middleware_context
  {
    protected middleware_context()
    {
      Env = new InMemoryCommunicationContext
      {
        PipelineData =
        {
          PipelineStage = new PipelineStage(Enumerable.Empty<ContributorCall>())
          {
            CurrentState = PipelineContinuation.Continue
          }
        }
      };
      Next = new NextMiddleware(() => NextCalled = true);
    }

    protected NextMiddleware Next { get; private set; }

    protected InMemoryCommunicationContext Env { get; }


    protected Func<ICommunicationContext, Task<PipelineContinuation>> Contributor(
      Func<ICommunicationContext, Task<PipelineContinuation>> contributor)
    {
      return env =>
      {
        ContributorCalled = true;
        return contributor(env);
      };
    }

    protected bool ContributorCalled { get; set; }
    protected bool NextCalled { get; set; }

    protected class NextMiddleware : AbstractMiddleware
    {
      readonly Action _onCall;

      public NextMiddleware(Action onCall)
      {
        _onCall = onCall;
      }
      public override Task Invoke(ICommunicationContext env)
      {
        _onCall();
        return Next.Invoke(env);
      }
    }
  }
}