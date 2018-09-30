using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class ContextDocumentConverter : JsonConverter<Context>
  {
    public override void WriteJson(JsonWriter writer, Context value, JsonSerializer serializer)
    {
      writer.Obj(() =>
      {
        writer.Obj("@context", () =>
        {
          writer.WritePropertyName("@vocab");
          writer.WriteValue(value.DefaultVocabulary);

          foreach (var c in value.Curies)
          {
            writer.WritePropertyName(c.Key);
            writer.WriteValue(c.Value);
          }

          foreach (var c in value.Classes)
          {
            writer.Obj(c.Key, () =>
            {
              writer.Obj("@context", () =>
              {
                writer.WritePropertyName("@vocab");
                writer.WriteValue(c.Value);
              });
            });
          }
        });
      });
    }

    public override Context ReadJson(JsonReader reader, Type objectType, Context existingValue, bool hasExistingValue,
      JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }

  public static class WriterExtensions
  {
    public static void Property<T>(this JsonWriter writer, string name, T value)
    {
      writer.WritePropertyName(name);
      writer.WriteValue(value);
      
    }
    public static void Obj(this JsonWriter writer, Action content)
    {
      writer.WriteStartObject();
      content();
      writer.WriteEndObject();
    }

    public static void Obj(this JsonWriter writer, string key, Action content)
    {
      writer.WritePropertyName(key);
      writer.Obj(content);
    }
  }
}