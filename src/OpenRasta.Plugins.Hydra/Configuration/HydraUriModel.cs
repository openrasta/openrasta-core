using System;
using OpenRasta.Configuration.MetaModel;

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
  }
}