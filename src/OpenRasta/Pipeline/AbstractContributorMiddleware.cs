using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public abstract class AbstractContributorMiddleware : IPipelineMiddlewareFactory
  {
    protected IPipelineMiddleware Next { get; set; }
    protected Func<ICommunicationContext, Task<PipelineContinuation>> Contributor { get; set; }
    public abstract Task Invoke(ICommunicationContext env);

    protected AbstractContributorMiddleware(Func<ICommunicationContext, Task<PipelineContinuation>> singleTapContributor)
    {
        if (singleTapContributor == null) throw new ArgumentNullException(nameof(singleTapContributor));
        Contributor = singleTapContributor;
    }

      public virtual IPipelineMiddleware Build(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    protected async Task<PipelineContinuation> InvokeSingleTap(ICommunicationContext env)
    {
        return await Contributor(env);
    }
  }
}
