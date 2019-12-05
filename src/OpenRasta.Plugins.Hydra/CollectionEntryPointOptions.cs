using OpenRasta.Plugins.Hydra.Schemas;

namespace OpenRasta.Plugins.Hydra
{
  public class CollectionEntryPointOptions
  {
    public string Uri { get; set; }
    public HydraCore.IriTemplate Search { get; set; }
  }
}