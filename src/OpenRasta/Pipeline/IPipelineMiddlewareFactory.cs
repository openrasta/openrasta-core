namespace OpenRasta.Pipeline
{
  public interface IPipelineMiddlewareFactory
  {
    IPipelineMiddleware Build(IPipelineMiddleware next);
  }
}
