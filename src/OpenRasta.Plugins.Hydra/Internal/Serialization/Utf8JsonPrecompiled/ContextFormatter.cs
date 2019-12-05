using OpenRasta.Plugins.Hydra.Schemas;
using Utf8Json;


namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class ContextFormatter : IJsonFormatter<HydraCore.Context>
  {
    public void Serialize(ref JsonWriter writer, HydraCore.Context value, IJsonFormatterResolver formatterResolver)
    {
      writer.WriteBeginObject();
      {
        writer.WritePropertyName("@context");
        writer.WriteBeginObject();

        {
          writer.WritePropertyName("@vocab");
          writer.WriteString(value.DefaultVocabulary);

          foreach (var c in value.Curies)
          {
            writer.WriteValueSeparator();
            writer.WritePropertyName(c.Key);
            writer.WriteString(c.Value.ToString());
          }

          foreach (var c in value.Classes)
          {
            writer.WriteValueSeparator();

            writer.WritePropertyName(c.Key);
            writer.WriteBeginObject();
            {
              writer.WritePropertyName("@context");
              writer.WriteBeginObject();
              {
                writer.WritePropertyName("@vocab");
                writer.WriteString(c.Value);
                
                writer.WriteEndObject();
              }
              writer.WriteEndObject();
            }
          }
          writer.WriteEndObject();
        }
        writer.WriteEndObject();
      }
    }

    public HydraCore.Context Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      throw new System.NotImplementedException();
    }
  }
}