using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public abstract class AbstractContributorMiddleware : IPipelineMiddlewareFactory, IPipelineMiddleware
  {
    protected IPipelineMiddleware Next { get; private set; } = Middleware.Identity;
    protected Func<ICommunicationContext, Task<PipelineContinuation>> ContributorInvoke { get; }
    public abstract Task Invoke(ICommunicationContext env);

    protected AbstractContributorMiddleware(ContributorCall call)
    {
      ContributorCall = call;
      ContributorInvoke = call.Action ?? throw new ArgumentNullException(nameof(call.Action));
    }

    public ContributorCall ContributorCall { get; }

    public virtual IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }
  }
}