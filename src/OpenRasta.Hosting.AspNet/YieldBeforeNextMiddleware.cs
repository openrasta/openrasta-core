using System.Threading.Tasks;
using System.Web;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class YieldBefore<T> : IPipelineMiddlewareFactory
  {
    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return new YieldBeforeNextMiddleware(typeof(T).Name).Compose(next);
    }
  }

  public class YieldBeforeNextMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly string _yieldName;

    public YieldBeforeNextMiddleware(string yieldName)
    {
      _yieldName = yieldName;
    }

    public async Task Invoke(ICommunicationContext env)
    {
      env.Yielder(_yieldName).SetResult(true);

      var currentContext = HttpContext.Current;
      var shouldContinue = await env.Resumer(_yieldName).Task;

      if (HttpContext.Current == null)
        HttpContext.Current = currentContext;
      if (shouldContinue)
        await Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }
}