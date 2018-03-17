using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;
using OpenRasta.Plugins.Caching.Providers;

namespace OpenRasta.Plugins.Caching.Configuration
{
    public class CacheConfigurationHandler : IMetaModelHandler
    {
        IDependencyResolver _resolver;

        public CacheConfigurationHandler(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void PreProcess(IMetaModelRepository repository)
        {
            var conf = repository.CustomRegistrations.OfType<CachingConfiguration>().FirstOrDefault();
            if (conf == null) return;

            _resolver.AddDependency(typeof(ICacheProvider), conf.CacheProviderType, DependencyLifetime.Singleton);
        }

        public void Process(IMetaModelRepository repository)
        {
        }
    }
}