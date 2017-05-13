using OpenRasta.Concordia;

namespace OpenRasta.Pipeline
{
    public interface IPipelineInitializer
    {
        IPipelineAsync Initialize(StartupProperties properties);
    }

}