namespace OpenRasta.Pipeline
{
  public interface IPipelineMiddlewareFactory
  {
    IPipelineMiddleware Compose(IPipelineMiddleware next);
  }
}
