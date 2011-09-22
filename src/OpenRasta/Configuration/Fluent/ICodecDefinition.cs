using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent
{
    public interface ICodecDefinition 
        : INoIzObject, 
          ICodec
    {
        ICodecWithMediaTypeDefinition ForMediaType(MediaType mediaType);
        ICodecParentDefinition And { get; }
    }
    public interface ICodecDefinition<TResource,TCodec> 
        : ICodecDefinition,
          ICodec<TResource,TCodec>
    {
        new ICodecWithMediaTypeDefinition<TResource, TCodec> ForMediaType(MediaType mediaType);
        new ICodecParentDefinition<TResource> And { get; }
    }
}