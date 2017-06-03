using System;
using System.Threading.Tasks;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Pipeline.Middleware
{
  public class DelegateBeforeNextMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly Action _beforeNext;

    public DelegateBeforeNextMiddleware(Action beforeNext)
    {
      _beforeNext = beforeNext;
    }

    public Task Invoke(ICommunicationContext env)
    {
      _beforeNext();
      return Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      this.Next = next;
      return this;
    }

    public IPipelineMiddleware Next { get; set; }
  }
}