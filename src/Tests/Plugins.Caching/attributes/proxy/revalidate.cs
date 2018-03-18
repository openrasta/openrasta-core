using OpenRasta.Plugins.Caching;
using OpenRasta.Plugins.Caching.Providers;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class revalidate
  {
    [Fact]
    public void on_proxy_only()
    {
      CacheResponse.GetResponseDirective(new CacheProxyAttribute {MustRevalidateWhenStale = true})
        .CacheDirectives.ShouldContain("must-revalidate");
    }
    [Fact]
    public void on_proxy_and_client()
    {
      CacheResponse.GetResponseDirective(
          new CacheProxyAttribute {MustRevalidateWhenStale = true},
          new CacheClientAttribute { MustRevalidateWhenStale = true})
        .CacheDirectives.ShouldContain("must-revalidate");
    }
    [Fact]
    public void on_proxy_not_on_client()
    {
      CacheResponse.GetResponseDirective(
          new CacheProxyAttribute {MustRevalidateWhenStale = true},
          new CacheClientAttribute { MustRevalidateWhenStale = false})
        .CacheDirectives.ShouldContain("proxy-revalidate");
    }
  }
}