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

    protected NextMiddleware Next { get; }

    protected InMemoryCommunicationContext Env { get; }


    protected ContributorCall Contributor(
      Func<ICommunicationContext, Task<PipelineContinuation>> contributor)
    {
      return new ContributorCall(
        NullPipelineContributor.Instance,
        env =>
        {
          ContributorCalled = true;
          return contributor(env);
        },
        string.Empty);
    }

    protected bool ContributorCalled { get; private set; }

    protected bool NextCalled { get; private set; }

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