using Newtonsoft.Json;
using OpenRasta.Configuration.Fluent;

namespace OpenRasta.Codecs.Newtonsoft.Json
{
  public static class FluentExtensions
  {
    public static ICodecDefinition NewtonsoftJson(
        this IHandlerForResourceWithUriDefinition root,
        JsonSerializerSettings settings = null)
    {
      return root.TranscodedBy<NewtonsoftJsonCodec>(settings);
    }
  }
}