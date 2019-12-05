using System;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas;

namespace OpenRasta.Plugins.Hydra.Configuration
{
  public class HydraUriModel
  {
    public HydraUriModel(UriModel uri)
    {
      Uri = uri;
    }

    public UriModel Uri { get; }
    public Type ResourceType { get; set; }
    public string EntryPointUri { get; set; }
    public HydraCore.IriTemplate SearchTemplate { get; set; }
  }
}