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
    }

    public InMemoryCommunicationContext Env { get; set; }

    protected Func<ICommunicationContext, Task<PipelineContinuation>> Contributor(
      Func<ICommunicationContext, Task<PipelineContinuation>> contributor)
    {
      return env =>
      {
        this.ContributorCalled = true;
        return contributor(env);
      };
    }

    public bool ContributorCalled { get; set; }
  }
}