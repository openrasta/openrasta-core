namespace OpenRasta.Pipeline
{
  public interface IPipelineMiddlewareFactory : IPipelineMiddleware
  {
    IPipelineMiddleware Build(IPipelineMiddleware next);
  }
}
