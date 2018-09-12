using System;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Configuration
{
  public class HydraUriModel
  {
    public HydraUriModel(UriModel uri)
    {
      Uri = uri;
    }

    public UriModel Uri { get; }
    public Type CollectionItemType { get; set; }
    public Type ResourceType { get; set; }
    public string EntryPointUri { get; set; }
    public IriTemplate SearchTemplate { get; set; }
  }
}