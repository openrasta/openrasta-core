using System;
using System.Collections.Generic;
using OpenRasta.OperationModel;

namespace OpenRasta.Plugins.Caching.Providers
{
    public interface ICacheProvider
    {
        CacheEntry Store(string key, TimeSpan maxAge, IEnumerable<OutputMember> localResult, IDictionary<string, string> varyHeaders);
        IEnumerable<CacheEntry> Get(string key);
    }
}