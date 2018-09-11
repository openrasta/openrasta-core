using System.Collections.Generic;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Plugins.Hydra.Schemas;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal
{
  public class Class : JsonLd.INode
  {
    public Class()
    {
      SupportedProperties = new List<SupportedProperty>();
    }

    [JsonProperty("@id")]
    public string Identifier { get; set; }

    [JsonProperty("supportedProperty")]
    public List<SupportedProperty> SupportedProperties { get; set; }
  }
}