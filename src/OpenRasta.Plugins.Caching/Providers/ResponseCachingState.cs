using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.Caching.Providers
{
    public class ResponseCachingState
    {
        public ResponseCachingState(IEnumerable<string> directives = null)
        {
            CacheDirectives = directives != null ? directives.ToList() : new List<string>();
        }

        public ICollection<string> CacheDirectives { get; }
    }
}