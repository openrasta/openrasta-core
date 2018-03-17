using System;
using System.Collections.Generic;
using OpenRasta.OperationModel;

namespace OpenRasta.Plugins.Caching.Providers
{
    public class CacheEntry
    {
        public DateTimeOffset ExpiresOn { get; private set; }
        public string Key { get; private set; }
        public IEnumerable<OutputMember> Value { get; private set; }
        public IDictionary<string, string> VaryHeaders { get; private set; }

        public CacheEntry(string key, DateTimeOffset expiresOn, IEnumerable<OutputMember> value, IDictionary<string,string> varyHeaders)
        {
            ExpiresOn = expiresOn;
            Key = key;
            Value = value;
            VaryHeaders = varyHeaders;
        }
    }
}