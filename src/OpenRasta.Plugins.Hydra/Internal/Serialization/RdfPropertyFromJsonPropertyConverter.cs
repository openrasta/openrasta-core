using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Configuration;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class RdfPropertyFromJsonPropertyConverter : JsonConverter
  {
    readonly IMetaModelRepository _models;

    public RdfPropertyFromJsonPropertyConverter(IMetaModelRepository models)
    {
      _models = models;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var pi = (JsonProperty) value;
      var resourceModel = _models.GetResourceModel(pi.DeclaringType);
      var hydraModel = resourceModel.Hydra();
      var vocab = hydraModel.Vocabulary;
      writer.WriteStartObject();
      writer.WritePropertyName("@id");
      writer.WriteValue(GetSchemaName(vocab, resourceModel, pi.PropertyName));
      writer.WritePropertyName("@type");
      writer.WriteValue("rdf:Property");
      if (TryGetRange(pi.PropertyType, out var range))
      {
        writer.WritePropertyName("range");
        writer.WriteValue(range);
      }

      writer.WriteEndObject();
    }

    bool TryGetRange(Type propertyType, out string range)
    {
      if (propertyType == typeof(string))
      {
        range = "xsd:string";
        return true;
      }

      range = null;
      return false;
    }

    string GetSchemaName(Vocabulary vocab, ResourceModel resourceModel, string propertyName)
    {
      var prefix = vocab.DefaultPrefix != null ? $"{vocab.DefaultPrefix}:" : "";
      return $"{prefix}{resourceModel.ResourceType.Name}/{propertyName}";
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType) => typeof(JsonProperty).IsAssignableFrom(objectType);
  }
}