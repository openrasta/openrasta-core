using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests.Plugins.Hydra.wf
{
  public class Ontology
  {
    [JsonProperty("@context")]
    public Dictionary<string, Datum> Context { get; set; } = new Dictionary<string, Datum>();

    [JsonIgnore]
    public string Scheme { get; set; }

    [JsonIgnore]
    public string Taxonomy { get; set; }

    public class Datum
    {
      [JsonProperty("@type")]
      public string Type { get; set; } = "Datum";

      public OntologyValue Value { get; set; }

      public class OntologyValue
      {
        [JsonProperty("rdfs:range")]
        public string DataType { get; set; }
      }
    }
  }
}