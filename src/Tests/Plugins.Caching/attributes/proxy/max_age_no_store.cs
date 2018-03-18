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
        Level = CacheLevel.DoNotCache
      });
      when_getting_response_caching();
    }

    [Fact]
    public void max_age_is_ignored()
    {
      exception.ShouldBeOfType<InvalidOperationException>();
    }
  }
}