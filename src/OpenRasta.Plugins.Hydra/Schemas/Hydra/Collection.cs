using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public static partial class Hydra
  {
    public class Collection<T>
    {
      public Collection(IEnumerable<T> enumerable)
      {
        Member = enumerable.ToArray();
        Manages = new CollectionManages()
        {
          Object = typeof(T).Name
        };
      }

      public CollectionManages Manages { get; }
      public T[] Member { get; set; }
      public int? TotalItems => Member?.Length;
    }
  }

  public class Collection : JsonLd.INode
  {
    public IriTemplate Search { get; set; }

    public CollectionManages Manages { get; } = new CollectionManages();
    public object[] Member { get; set; }
    public int? TotalItems => Member?.Length;

    [JsonProperty("@id")]
    public Uri Identifier { get; set; }
  }

  public class CollectionManages
  {
    public string Property => "rdf:type";
    public string Object { get; set; }
  }
}