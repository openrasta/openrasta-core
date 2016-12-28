using OpenRasta.Concordia;

namespace OpenRasta.Pipeline
{
    public interface IPipelineInitializer
    {
        void Initialize(StartupProperties properties);
    }

}