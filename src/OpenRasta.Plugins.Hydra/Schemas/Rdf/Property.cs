using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class Rdf
  {
    public class Property : JsonLd.IBlankNode
    {
      public string Range { get; set; }

      [JsonProperty("@id")]
      public string Identifier { get; set; }
    }
  }
}