using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  static class Nodes
  {
    public static byte[] BeginObjectContext;
    public static byte[] IdProperty;
    static byte[] TypeProperty;

    static Nodes()
    {
      var writer = new JsonWriter();
      writer.WriteBeginObject();
      writer.WritePropertyName("@context");
      BeginObjectContext = writer.ToUtf8ByteArray();
      
      
      writer = new JsonWriter();
      writer.WritePropertyName("@id");
      IdProperty = writer.ToUtf8ByteArray();
      
      writer = new JsonWriter();
      writer.WritePropertyName("@type");
      TypeProperty = writer.ToUtf8ByteArray();
    }
    
  }
}