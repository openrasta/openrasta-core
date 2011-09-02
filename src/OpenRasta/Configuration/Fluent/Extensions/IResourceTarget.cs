JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface IResourceTarget : IFluentTarget
    {
        ResourceModel Resource { get; }
    }
}