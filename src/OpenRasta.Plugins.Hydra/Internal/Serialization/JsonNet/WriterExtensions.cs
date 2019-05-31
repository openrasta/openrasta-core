using System;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.JsonNet
{
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