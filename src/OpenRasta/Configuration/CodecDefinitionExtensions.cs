using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.Web;

namespace OpenRasta.Configuration
{
    public static class CodecDefinitionExtensions
    {
      public static ICodecDefinition Buffered(this ICodecDefinition codecDefinition)
      {
        var codecModel = codecDefinition is CodecMediaTypeDefinition mtDev
          ? mtDev.Codec
          : codecDefinition is CodecDefinition dev
            ? dev.Codec
            : null;
        if (codecModel != null) codecModel.IsBuffered = true;
        
        return codecDefinition;
      }
        public static ICodecWithMediaTypeDefinition ForMediaType(this ICodecDefinition codecDefinition, string mediaType)
        {
            return codecDefinition.ForMediaType(new MediaType(mediaType));
        }
    }
}