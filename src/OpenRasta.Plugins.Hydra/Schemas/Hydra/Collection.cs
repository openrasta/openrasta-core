using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class Collection : JsonLd.INode
  {
    public IriTemplate Search { get; set; }

    public CollectionManages Manages { get; } = new CollectionManages();
    public object[] Member { get; set; }
    public int? TotalItems => Member?.Length;

    [JsonProperty("@id")]
    public Uri Identifier { get; set; }
  }
}