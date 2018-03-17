using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;

namespace OpenRasta.Plugins.Caching.Providers
{
    public class ResponseCachingState
    {
        public ResponseCachingState(IEnumerable<string> directives = null)
        {
            CacheDirectives = directives != null ? directives.ToList() : new List<string>();
        }
        public TimeSpan? LocalCacheMaxAge { get; set; }
        public bool LocalCacheEnabled { get; set; }

        public ICollection<string> CacheDirectives { get; set; }

        public IEnumerable<OutputMember> LocalResult { get; set; }
    }
}