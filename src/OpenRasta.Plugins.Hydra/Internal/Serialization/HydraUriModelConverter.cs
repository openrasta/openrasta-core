using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class HydraUriModelConverter : JsonConverter<HydraUriModel>
  {
    readonly Uri _baseUri;

    public HydraUriModelConverter(Uri baseUri)
    {
      _baseUri = baseUri;
    }

    public override void WriteJson(JsonWriter writer, HydraUriModel value, JsonSerializer serializer)
    {
      writer.WriteValue(new Uri(_baseUri,new Uri(value.EntryPointUri, UriKind.RelativeOrAbsolute)));
    }

    public override HydraUriModel ReadJson(JsonReader reader, Type objectType, HydraUriModel existingValue, bool hasExistingValue,
      JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}