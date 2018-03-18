using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class proxy_max_age : contexts.attributes
  {
    public proxy_max_age()
    {
      given_attribute(proxy: new CacheProxyAttribute {MaxAge = "00:01:00"});
      when_getting_response_caching();
    }

    [Fact]
    public void response_has_max_age()
    {
      cache.CacheDirectives.ShouldContain("max-age=60");
    }
  }
}