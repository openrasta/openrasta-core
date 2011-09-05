using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface ICodecTarget : IResourceTarget
    {
        CodecModel Codec { get; }
    }
}