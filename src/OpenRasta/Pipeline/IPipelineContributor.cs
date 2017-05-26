namespace OpenRasta.Pipeline
{
    public interface IPipelineContributor
    {
        void Initialize(IPipeline pipelineRunner);
    }
}