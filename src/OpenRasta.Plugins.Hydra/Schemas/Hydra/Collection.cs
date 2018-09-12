using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class Collection : JsonLd.INode
  {
    public Collection()
    {
      
    }
    public Collection(Type itemType, HydraUriModel id)
    {
      Manages = itemType;
      Identifier = id;
      Search = id.SearchTemplate;
    }

    public IriTemplate Search { get; set; }

    [JsonProperty("@id")]
    public HydraUriModel Identifier { get; set; }

    public Type Manages { get; }
    public object[] Member { get; set; }
    public int? TotalItems => Member?.Length;
  }
}