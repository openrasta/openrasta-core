namespace OpenRasta.Configuration.Fluent
{
    public interface ICodecWithMediaTypeDefinition : ICodecDefinition, IMediaType
    {
        ICodecWithMediaTypeDefinition ForExtension(string extension);
    }
    public interface ICodecWithMediaTypeDefinition<TResource,TCodec> 
        : ICodecWithMediaTypeDefinition, IMediaType<TResource,TCodec>
    {
        new ICodecWithMediaTypeDefinition<TResource,TCodec> ForExtension(string extension);
    }
}