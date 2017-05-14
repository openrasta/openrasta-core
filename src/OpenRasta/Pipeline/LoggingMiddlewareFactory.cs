namespace OpenRasta.Pipeline
{
  public class LoggingMiddlewareFactory : IPipelineMiddlewareFactory
  {
    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return new LoggingMiddleware(next);
    }
  }
}