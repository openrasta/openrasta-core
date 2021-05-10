using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tests.Plugins.Hydra.wf
{
  public class Collection<T> : Collection
  {
    public Collection(string name, params T[] hasPart)
    {
      Name = name;
      HasPart = hasPart?.ToList() ?? new List<T>();
    }
    public List<T> HasPart { get; }
  }

  public class SiteMap : Collection<Collection<Variable>>
  {
    public SiteMap(string name, params Collection<Variable>[] hasPart) : base(name, hasPart)
    {
    }
  }
  public abstract class Collection
  {
    public string Name { get; protected set; }
    [JsonProperty("@type")] public string Type => "Collection";
  }
}
