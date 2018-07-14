using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class EntryPoint : IJsonLdDocument
  {
    [JsonProperty("collection")]
    public List<Collection> Collections { get; set; }
  }
}