using System;
using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.attributes.proxy
{
  public class max_age_no_store : contexts.attributes
  {
    public max_age_no_store()
    {
      given_attribute(proxy: new CacheProxyAttribute
      {
        MaxAge = "00:01:00",
        Level = ProxyCacheLevel.None
      });
      when_getting_response_caching();
    }

    [Fact]
    public void error_is_generated()
    {
      exception.ShouldBeOfType<InvalidOperationException>();
    }
  }
}