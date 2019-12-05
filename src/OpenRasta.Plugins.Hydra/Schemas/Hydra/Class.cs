using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class HydraCore
  {
    public class Class
    {
      public Class()
      {
        SupportedProperties = new List<SupportedProperty>();
      }

      [JsonProperty("@id")]
      public string Identifier { get; set; }

      [JsonProperty("supportedProperty")]
      public List<SupportedProperty> SupportedProperties { get; set; }

      [JsonProperty("supportedOperation")]
      public List<Operation> SupportedOperations { get; set; }
    }
  }
}