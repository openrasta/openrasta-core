using System;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
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
}