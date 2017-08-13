using OpenRasta.DI.Internal;

namespace OpenRasta.Pipeline
{
    public interface IContextStoreDependencyCleaner
    {
        void Destruct(DependencyRegistration registration, object instance);
    }
}