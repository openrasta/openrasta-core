using System;
using Newtonsoft.Json;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  class JsonLdTypeRefConverter : JsonConverter
  {
    readonly IMetaModelRepository _models;

    public JsonLdTypeRefConverter(IMetaModelRepository models)
    {
      _models = models;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var type = (Type) value;
      var hasResourceModel = _models.TryGetResourceModel(type, out var model);
      var vocab = model?.Hydra().Vocabulary;
      if (!hasResourceModel || vocab == null)
      {
        writer.WriteValue(type.AssemblyQualifiedName);
        return;
      }

      writer.WriteStartObject();
      writer.WritePropertyName("property");
      writer.WriteValue("rdf:Type");
      writer.WritePropertyName("object");
      writer.WriteValue($"{vocab.DefaultPrefix}:{type.Name}");
      writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(Type).IsAssignableFrom(objectType);
    }
  }
}