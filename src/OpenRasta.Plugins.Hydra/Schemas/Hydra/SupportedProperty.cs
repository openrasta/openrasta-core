using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class SupportedProperty : JsonLd.IBlankNode
  {
    public SupportedProperty(JsonProperty jsonProperty)
    {
      Property = jsonProperty;
    }

    public JsonProperty Property { get; set; }
  }
  
}