using OpenRasta.Pipeline;

namespace Tests.Pipeline.Middleware.Examples
{
  public class TrailerMiddleware : IPipelineMiddlewareFactory
  {
    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return new SimpleMiddleware(next);
    }
  }
}