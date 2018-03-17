using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Pipeline;
using OpenRasta.Plugins.Caching.Providers;

namespace OpenRasta.Plugins.Caching.Configuration
{
    public class CachingBuilder
    {
        CachingConfiguration _conf = new CachingConfiguration();

        public CachingBuilder(IFluentTarget uses)
        {
            // TODO: Dragons dragons, if no resolver registered we should add a custom registration
            // so we can support both 2.1 and 2.0
            DependencyManager.GetService<IDependencyResolver>().AddDependency<IMetaModelHandler, CacheConfigurationHandler>(DependencyLifetime.Transient);

            uses.Repository.CustomRegistrations.Add(new DependencyRegistrationModel(
                                                        typeof(IOperationInterceptor),
                                                        typeof(CachingInterceptor),
                                                        DependencyLifetime.Transient));

            RegisterContributor<CachingContributor>(uses);
            RegisterContributor<ConditionalEtagContributor>(uses);
            RegisterContributor<ConditionalLastModifiedContributor>(uses);
            RegisterContributor<EntityEtagContributor>(uses);
            RegisterContributor<EntityLastModified>(uses);
            uses.Repository.CustomRegistrations.Add(_conf);
        }

        static void RegisterContributor<T>(IFluentTarget uses) where T:IPipelineContributor
        {
            uses.Repository.CustomRegistrations.Add(new DependencyRegistrationModel(
                                                        typeof(IPipelineContributor),
                                                        typeof(T),
                                                        DependencyLifetime.Transient));
        }

        public CachingBuilder Auto()
        {
            _conf.Automatic = true;
            return this;
        }
        public CachingBuilder CacheProvider<T>() where T: ICacheProvider
        {
            _conf.CacheProviderType = typeof(T);
            return this;
        }

    }
}