using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Providers;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
    public class CachingInterceptor : IOperationInterceptor
    {
        readonly ICacheProvider _cache;
        readonly PipelineData _data;
        readonly IRequest _request;
        IEnumerable<OutputMember> _cachedValue;
        CacheProxyAttribute _proxy;
        CacheClientAttribute _client;

        public CachingInterceptor(ICommunicationContext context, ICacheProvider cache)
        {
            _request = context.Request;
            _data = context.PipelineData;
            _cache = cache;
        }

        public bool AfterExecute(IOperation operation, IEnumerable<OutputMember> outputMembers)
        {
            var responseCache = CacheResponse.GetResponseDirective(_proxy, _client);
            responseCache.LocalResult = outputMembers.ToList();

            _data[Keys.RESPONSE_CACHE] = responseCache;
            return true;
        }

        public bool BeforeExecute(IOperation operation)
        {
            _proxy = operation.FindAttribute<CacheProxyAttribute>();
            _client = operation.FindAttribute<CacheClientAttribute>();

            var cacheEntry = TryGetValidCacheEntry(_request.Uri.AbsolutePath);

            if (cacheEntry != null)
            {
                _cachedValue = cacheEntry.Value;
            }
            return true;
        }

        public Func<IEnumerable<OutputMember>> RewriteOperation(Func<IEnumerable<OutputMember>> operationBuilder)
        {
            return () => _cachedValue ?? operationBuilder();
        }

        CacheEntry TryGetValidCacheEntry(string absolutePath)
        {
            var key = _cache.Get(absolutePath);
            return key.Where(VaryHeadersCompatible).OrderBy(x => x.ExpiresOn).FirstOrDefault();
        }

        bool VaryHeadersCompatible(CacheEntry cacheEntry)
        {
            return true;
        }
    }
}