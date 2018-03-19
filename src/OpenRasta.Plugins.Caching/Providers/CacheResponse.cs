using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Plugins.Caching.Providers
{
  public static class CacheResponse
  {
    public static ResponseCachingState GetResponseDirective(
      CacheProxyAttribute proxy = null,
      CacheClientAttribute client = null)
    {
      ValidateProxyAttribute(proxy);

      var instructions =
        CacheVisibility(proxy, client)
          .Concat(CacheRevalidation(proxy, client))
          .Concat(CacheMaxAge(proxy, client));

      return new ResponseCachingState(instructions);
    }

    static void ValidateProxyAttribute(CacheProxyAttribute proxy)
    {
      if (proxy?.MaxAge != null && proxy.Level == CacheLevel.DoNotCache)
        throw new InvalidOperationException("Cannot set MaxAge to a value and have the proxy cache disabled");
    }

    static IEnumerable<string> CacheMaxAge(CacheProxyAttribute proxy, CacheClientAttribute client)
    {
      TimeSpan proxyAge = TimeSpan.MinValue, browserAge = TimeSpan.MinValue;
      if (proxy?.MaxAge != null) TimeSpan.TryParse(proxy.MaxAge, out proxyAge);
      if (client?.MaxAge != null) TimeSpan.TryParse(client.MaxAge, out browserAge);

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
      if (proxy?.MustRevalidateWhenStale == true && client?.MustRevalidateWhenStale == false)
        yield return "proxy-revalidate";

      if (proxy?.MustRevalidateWhenStale == true || proxy?.MustRevalidateWhenStale == true)
        yield return "must-revalidate";
    }

    static IEnumerable<string> CacheVisibility(CacheProxyAttribute proxy, CacheClientAttribute client)
    {
      if (proxy == null && client == null)
        yield break;

      if (proxy?.Level == CacheLevel.Everything)
        yield return "public";
      else if ((proxy == null || proxy.Level == CacheLevel.DoNotCache) &&
               (client?.Level == CacheLevel.Cacheable))
        yield return "private";
      else if (proxy?.Level == CacheLevel.DoNotCache && client?.Level == CacheLevel.DoNotCache)
        yield return "no-cache";
    }
  }
}