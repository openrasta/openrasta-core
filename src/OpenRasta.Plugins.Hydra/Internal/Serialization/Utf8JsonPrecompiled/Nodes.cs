using Utf8Json;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  static class Nodes
  {
    public static readonly byte[] BeginObjectContext;
    public static readonly byte[] IdProperty;
    public static readonly byte[] TypeProperty;

    static Nodes()
    {
      var writer = new JsonWriter();
      writer.WriteBeginObject();
      writer.WritePropertyName("@context");
      var  bytes = writer.ToUtf8ByteArray();
      BeginObjectContext = bytes;
      
      writer = new JsonWriter();
      writer.WritePropertyName("@id");
      IdProperty = writer.ToUtf8ByteArray();
      
      writer = new JsonWriter();
      writer.WritePropertyName("@type");
      TypeProperty = writer.ToUtf8ByteArray();
    }
    
  }
}