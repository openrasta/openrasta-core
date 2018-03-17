using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class empty : contexts.attributes
  {
    public empty()
    {
      given_attribute(proxy: new CacheProxyAttribute());
      when_getting_response_caching();
    }

    [Fact]
    public void should_be_private()
    {
      cache.CacheDirectives.ShouldBeEmpty();
    }
  }
}