using System;
using OpenRasta.Plugins.Caching.Providers;

namespace OpenRasta.Plugins.Caching.Configuration
{
    public class CachingConfiguration
    {
        public CachingConfiguration()
        {
            CacheProviderType = typeof(InMemoryCacheProvider);
        }
        public bool Automatic { get; set; }

        public Type CacheProviderType { get; set; }
    }
}