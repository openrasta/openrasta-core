using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class CompilerContext
  {
    public CompilerContext(IMetaModelRepository metaModel, ResourceModel resource)
    {
      MetaModel = metaModel;
      Resource = resource;
    }

    public IMetaModelRepository MetaModel { get; set; }
    public ResourceModel Resource { get; set; }
  }
}