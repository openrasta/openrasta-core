using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Configuration;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class HydraUriModelConverter : JsonConverter
  {
    readonly Uri _baseUri;
    readonly IUriResolver _uris;

    public HydraUriModelConverter(IUriResolver uris, Uri baseUri)
    {
      _uris = uris;
      _baseUri = baseUri;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      writer.WriteValue(_uris.CreateUriFor(((HydraUriModel) value).ResourceType, _baseUri));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(HydraUriModel).IsAssignableFrom(objectType);
    }
  }
}