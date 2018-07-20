using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Schemas.Hydra
{
  public class Collection : IIriNode
  {
    public Collection(Type itemType, HydraUriModel id)
    {
      Manages = itemType;
      Identifier = id;
    }

    [JsonProperty("@id")]
    public HydraUriModel Identifier { get; set; }

    public Type Manages { get; }
  }
}