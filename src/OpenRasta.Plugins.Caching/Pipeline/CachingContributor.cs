using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching.Providers;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
    public class CachingContributor : IPipelineContributor
    {
        const string CACHE_CONTROL = "cache-control";
        ICacheProvider _cache;
        ILogger _log;

        public CachingContributor(ICacheProvider cache, ILogger log)
        {
            _cache = cache;
            _log = log;
        }

        public void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(WriteCacheDirectives)
                .After<KnownStages.IOperationResultInvocation>()
                .And.Before<KnownStages.IResponseCoding>();
        }

        PipelineContinuation WriteCacheDirectives(ICommunicationContext arg)
        {
            object value;
            if (!arg.PipelineData.TryGetValue(Keys.RESPONSE_CACHE, out value))
                return PipelineContinuation.Continue;

            var responseCache = (ResponseCachingState)value;

            if (responseCache.CacheDirectives.Any())
                arg.Response.Headers[CACHE_CONTROL] = responseCache.CacheDirectives.JoinString(", ");

            if (CanCache(responseCache, arg))
            {
                _cache.Store(arg.Request.Uri.AbsolutePath, responseCache.LocalCacheMaxAge.Value, responseCache.LocalResult, ReadVaryHeaders(arg));
            }
            return PipelineContinuation.Continue;
        }

        CacheKey GenerateCacheKey(ICommunicationContext env)
        {
            return new CacheKey(env.Request.Uri.AbsolutePath, ReadVaryHeaders(env));
        }

        IDictionary<string, string> ReadVaryHeaders(ICommunicationContext env)
        {
            return new Dictionary<string, string>();
        }

        bool CanCache(ResponseCachingState state, ICommunicationContext env)
        {
            // we only cache 200 to GET for now
            return state.LocalCacheEnabled &&
                   env.Request.HttpMethod == "GET" &&
                   env.Response.StatusCode == 200;
                ; //&& (ContainsExpire(response) ||
                                            //   ContainsMaxAge(response) ||
                                            //   ContainsSharedMaxAge(response));

        }

        bool ContainsMaxAge(IResponse response)
        {
            return response.Headers[CACHE_CONTROL].Split(',').Any(_ => _.Contains("max-age"));
        }

        bool ContainsSharedMaxAge(IResponse response)
        {
            return response.Headers[CACHE_CONTROL].Split(',').Any(_ => _.Contains("s-maxage"));
        }

        bool ContainsExpire(IResponse response)
        {
            return response.Headers["expires"] != null;
        }
    }
}