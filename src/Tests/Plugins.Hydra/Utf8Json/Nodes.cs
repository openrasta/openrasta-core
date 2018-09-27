using Utf8Json;

namespace Tests.Plugins.Hydra.Utf8Json
{
  static class Nodes
  {
    public static byte[] BeginObjectContextComa;

    static Nodes()
    {
      var writer = new JsonWriter();
      writer.WriteBeginObject();
      writer.WritePropertyName("@context");
      writer.WriteString("/.hydra/context.jsonld");
      writer.WriteValueSeparator();
      BeginObjectContextComa = writer.ToUtf8ByteArray();
    }
  }
}