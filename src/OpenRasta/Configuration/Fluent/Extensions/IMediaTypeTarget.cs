using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface IMediaTypeTarget : ICodecTarget
    {
        MediaTypeModel MediaType { get; }
    }
}