//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using OpenRasta.OperationModel;
//using System.Runtime.Caching;
//
//namespace OpenRasta.Caching.Providers
//{
//    public class SystemCacheProvider : ICacheProvider
//    {
//        ObjectCache _cache = new MemoryCache("openrasta.caching");
//        public CacheEntry Store(string key, TimeSpan maxAge, IEnumerable<OutputMember> localResult, IDictionary<string, string> varyHeaders)
//        {
//            var now = ServerClock.UtcNow();
//            var entry = new CacheEntry(key, now + maxAge, localResult, varyHeaders);
//
//            var keyList = (HashSet<string>)_cache.AddOrGetExisting(key, new HashSet<string>(), ObjectCache.InfiniteAbsoluteExpiration);
//            keyList.Add(entry.ToString());
//            _cache.Set(entry.ToString(), entry, entry.ExpiresOn);
//            return entry;
//        }
//
//        public IEnumerable<CacheEntry> Get(string key)
//        {
//            var list = _cache.Get(key) as HashSet<string>;
//            if (list == null) return Enumerable.Empty<CacheEntry>();
//
//            return list.Select(x => _cache.Get(x)).Cast<CacheEntry>().ToList();
//        }
//    }
//}