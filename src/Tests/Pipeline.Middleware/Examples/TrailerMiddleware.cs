using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Interception
{
  public class TrailerMiddleware : IPipelineMiddlewareFactory
  {
    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return new SimpleMiddleware(next);
    }
  }
}