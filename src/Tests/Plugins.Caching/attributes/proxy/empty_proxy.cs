using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class empty_proxy : contexts.attributes
  {
    public empty_proxy()
    {
      given_attribute(proxy: new CacheProxyAttribute());
      when_getting_response_caching();
    }

    [Fact]
    public void cacheable_by_default()
    {
      cache.CacheDirectives.ShouldBeEmpty();
    }
  }
}