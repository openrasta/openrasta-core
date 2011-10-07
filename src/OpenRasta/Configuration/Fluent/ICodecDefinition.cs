using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent
{
    public interface ICodecDefinition 
        : INoIzObject, 
          IRepeatableDefinition<ICodecParentDefinition>,
          ICodec
    {
        ICodecWithMediaTypeDefinition ForMediaType(MediaType mediaType);
    }
}