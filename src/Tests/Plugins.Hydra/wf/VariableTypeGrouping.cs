using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests.Plugins.Hydra.wf
{
  public class VariableTypeGrouping
  {
    [JsonProperty("key")]
    public int Order { get; set; }
    [JsonProperty("value")]
    public string Name { get; set; }

    public static implicit operator VariableTypeGrouping(KeyValuePair<int, string> kv)
    {
      return new VariableTypeGrouping {Order = kv.Key, Name = kv.Value};
    }
  }
}