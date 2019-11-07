using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra
{
  public class CollectionEntryPointOptions
  {
    public string Uri { get; set; }
    public IriTemplate Search { get; set; }
  }
}