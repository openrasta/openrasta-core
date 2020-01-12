using System;
using Newtonsoft.Json;

namespace Tests.Plugins.Hydra.wf
{
  public class LandRegVariableType
  {
    protected LandRegVariableType(string scheme, string taxonomy, string classification, string name)
    {
      Scheme = scheme;
      Taxonomy = taxonomy;
      Classification = classification;
      Name = name;
      Type = new Uri("http://localhost/blaaaaah");
    }

    [JsonProperty("@type")]
    public Uri Type { get; set; }

    [JsonIgnore]
    public string Scheme { get; set; }
    [JsonIgnore]
    public string Taxonomy { get; set; }
    [JsonIgnore]
    public string Classification { get; set; }
    [JsonIgnore]
    public string Name { get; set; }
  }
}