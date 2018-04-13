using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.AspNet
{
  public class YieldBefore<T> : IPipelineMiddlewareFactory
  {
    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return new YieldBeforeNextMiddleware(typeof(T).Name).Compose(next);
    }
  }
}