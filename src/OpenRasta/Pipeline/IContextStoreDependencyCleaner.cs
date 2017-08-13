using OpenRasta.DI.Internal;

namespace OpenRasta.Pipeline
{
    public interface IContextStoreDependencyCleaner
    {
        void UnregisterTemporaryRegistration(DependencyRegistration registration, object instance);
    }
}