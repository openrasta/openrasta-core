using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Schemas
{

  public static partial class HydraCore
  {
    public class EntryPoint
    {
      [JsonProperty("collection")]
      public List<CollectionWithIdentifier> Collections { get; set; }
    }
  }
}