namespace OpenRasta.Pipeline
{
  public interface IPipelineMiddleware : IPipelineComponent
  {
    IPipelineComponent Build(IPipelineComponent next);
  }
}