namespace OpenRasta.Configuration.Fluent
{
    public interface ICodecWithMediaTypeDefinition : ICodecDefinition, IMediaType
    {
        ICodecWithMediaTypeDefinition ForExtension(string extension);
    }
}