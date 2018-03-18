using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.Caching.Providers
{
    public static class CacheResponse
    {
        public static ResponseCachingState GetResponseDirective(
            CacheProxyAttribute proxy,
            CacheClientAttribute client)
        {
            ValidateProxyAttribute(proxy);

            var instructions = CacheVisibility(proxy, client)
                .Concat(CacheRevalidation(proxy, client))
                .Concat(CacheMaxAge(proxy, client));

          return new ResponseCachingState(instructions);
        }

        static void SetupLocalCaching(CacheProxyAttribute proxy, ResponseCachingState response)
        {
            TimeSpan parsedMaxAge;
            if (proxy.MaxAge != null && TimeSpan.TryParse(proxy.MaxAge, out parsedMaxAge))
            {
                response.LocalCacheEnabled = true;
                response.LocalCacheMaxAge = parsedMaxAge;
            }
        }

        static void ValidateProxyAttribute(CacheProxyAttribute proxy)
        {
            if (proxy != null && proxy.MaxAge != null && proxy.Level == CacheLevel.DoNotCache)
                throw new InvalidOperationException("Cannot set MaxAge to a value and have the proxy cache disabled");
        }

        static IEnumerable<string> CacheMaxAge(CacheProxyAttribute proxy, CacheClientAttribute client)
        {
            TimeSpan proxyAge = TimeSpan.MinValue, browserAge = TimeSpan.MinValue;
            if (proxy != null && proxy.MaxAge != null) TimeSpan.TryParse(proxy.MaxAge, out proxyAge);
            if (client != null && client.MaxAge != null) TimeSpan.TryParse(client.MaxAge, out browserAge);

            if (proxyAge == TimeSpan.MinValue && browserAge == TimeSpan.MinValue)
                yield break;

            if (proxyAge != TimeSpan.MinValue && browserAge != TimeSpan.MinValue)
                yield return "s-max-age=" + proxyAge.TotalSeconds;

            yield return "max-age=" + (proxyAge != TimeSpan.MinValue
                                           ? proxyAge.TotalSeconds
                                           : browserAge.TotalSeconds);
        }

        static IEnumerable<string> CacheRevalidation(CacheProxyAttribute proxy, CacheClientAttribute client)
        {
            if (proxy != null && proxy.MustRevalidate && client != null && client.MustRevalidate)
                yield return "must-revalidate";
            else if (proxy != null && proxy.MustRevalidate)
                yield return "proxy-revalidate";
        }

        static IEnumerable<string> CacheVisibility(CacheProxyAttribute proxy, CacheClientAttribute client)
        {
            if (proxy == null && client == null)
                yield break;
            if (proxy != null && proxy.Level == CacheLevel.Everything)
                yield return "public";
            else if ((proxy == null || proxy.Level == CacheLevel.DoNotCache) && 
                (client == null || client.Level == CacheLevel.Cacheable))
                yield return "private";
            else if (proxy != null && client != null && proxy.Level == CacheLevel.DoNotCache && client.Level == CacheLevel.DoNotCache)
                yield return "no-cache";
        }
    }
}