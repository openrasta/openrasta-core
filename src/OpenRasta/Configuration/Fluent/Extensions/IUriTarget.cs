using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface IUriTarget : IResourceTarget
    {
        UriModel Uri { get; }
    }
}