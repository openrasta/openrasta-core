using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class EntryPoint : JsonLd.INode
  {
    [JsonProperty("collection")]
    public List<Collection> Collections { get; set; }
  }
}