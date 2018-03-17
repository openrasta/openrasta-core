using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.Caching.Providers
{
    public static class CacheResponse
    {
        public static ResponseCachingState GetResponseDirective(
            CacheProxyAttribute proxy,
            CacheBrowserAttribute browser)
        {
            ValidateProxyAttribute(proxy);

            var instructions = CacheVisibility(proxy, browser)
                .Concat(CacheRevalidation(proxy, browser))
                .Concat(CacheMaxAge(proxy, browser));

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
            if (proxy != null && proxy.MaxAge != null && proxy.Level == ProxyCacheLevel.None)
                throw new InvalidOperationException("Cannot set MaxAge to a value and have the proxy cache disabled");
        }

        static IEnumerable<string> CacheMaxAge(CacheProxyAttribute proxy, CacheBrowserAttribute browser)
        {
            TimeSpan proxyAge = TimeSpan.MinValue, browserAge = TimeSpan.MinValue;
            if (proxy != null && proxy.MaxAge != null) TimeSpan.TryParse(proxy.MaxAge, out proxyAge);
            if (browser != null && browser.MaxAge != null) TimeSpan.TryParse(browser.MaxAge, out browserAge);

            if (proxyAge == TimeSpan.MinValue && browserAge == TimeSpan.MinValue)
                yield break;

            if (proxyAge != TimeSpan.MinValue && browserAge != TimeSpan.MinValue)
                yield return "s-max-age=" + proxyAge.TotalSeconds;

            yield return "max-age=" + (proxyAge != TimeSpan.MinValue
                                           ? proxyAge.TotalSeconds
                                           : browserAge.TotalSeconds);
        }

        static IEnumerable<string> CacheRevalidation(CacheProxyAttribute proxy, CacheBrowserAttribute browser)
        {
            if (proxy != null && proxy.MustRevalidate && browser != null && browser.MustRevalidate)
                yield return "must-revalidate";
            else if (proxy != null && proxy.MustRevalidate)
                yield return "proxy-revalidate";
        }

        static IEnumerable<string> CacheVisibility(CacheProxyAttribute proxy, CacheBrowserAttribute browser)
        {
            if (proxy == null && browser == null)
                yield break;
            if (proxy != null && proxy.Level == ProxyCacheLevel.Everything)
                yield return "public";
            else if ((proxy == null || proxy.Level == ProxyCacheLevel.None) && 
                (browser == null || browser.Level == BrowserCacheLevel.Default))
                yield return "private";
            else if (proxy != null && browser != null && proxy.Level == ProxyCacheLevel.None && browser.Level == BrowserCacheLevel.None)
                yield return "no-cache";
        }
    }
}