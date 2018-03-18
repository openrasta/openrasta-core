using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class no_store : contexts.attributes
  {
    // For proxies, Store = false maps to private.
    // http://tools.ietf.org/html/draft-ietf-httpbis-p6-cache-16#section-3.2.2
    // The private response directive indicates that the response message
    // is intended for a single user and MUST NOT be stored by a shared
    // cache.  A private cache MAY store the response.
    public no_store()
    {
      given_attribute(proxy: new CacheProxyAttribute {Level = CacheLevel.DoNotCache});
      when_getting_response_caching();
    }

    [Fact]
    public void response_not_cached_by_proxies()
    {
      cache.CacheDirectives.ShouldContain("private");
    }
  }
}