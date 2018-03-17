using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;

namespace OpenRasta.Plugins.Caching.Providers
{
    public class InMemoryCacheProvider : ICacheProvider
    {
        readonly List<CacheEntry> _localCache = new List<CacheEntry>();



        public CacheEntry Store(string key, TimeSpan maxAge, IEnumerable<OutputMember> localResult, IDictionary<string, string> varyHeaders)
        {
            lock(_localCache)
            {
                var cacheEntry = new CacheEntry(key, ServerClock.UtcNow() + maxAge, localResult, varyHeaders);
                _localCache.Add(cacheEntry);
                return cacheEntry;
            }
        }

        public IEnumerable<CacheEntry> Get(string key)
        {
            lock(_localCache)
            {
                var all = _localCache.Where(x => x.Key == key).ToList();
                var now = ServerClock.UtcNow();
                var expired = all.Where(x => x.ExpiresOn < now).ToList();
                expired.ForEach(_ =>
                {
                    all.Remove(_);
                    _localCache.Remove(_);
                });
                return all;
            }
        }
    }
}