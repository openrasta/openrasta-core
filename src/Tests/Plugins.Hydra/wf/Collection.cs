using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tests.Plugins.Hydra.wf
{
  public class Collection<T> : Collection
  {
    public Collection(string name, IEnumerable<T> hasPart = null)
    {
      Name = name;
      HasPart = hasPart?.ToList() ?? new List<T>();
    }
    public List<T> HasPart { get; }
  }
  
  public abstract class Collection
  {
    public string Name { get; protected set; }
    [JsonProperty("@type")] public string Type => "Collection";
  }
}
