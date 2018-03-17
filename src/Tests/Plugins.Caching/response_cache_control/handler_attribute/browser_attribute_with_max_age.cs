using OpenRasta.Configuration;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.response_cache_control.handler_attribute
{
  public class browser_attribute_with_max_age : caching
  {
    public browser_attribute_with_max_age()
    {
      given_has(_ => _.ResourcesOfType<Resource>()
        .AtUri("/").Named("CacheBrowser")
        .HandledBy<CachingHandler>()
        .AsJsonDataContract().ForMediaType("*/*"));
      when_executing_request("/");
    }

    [Fact]
    public void cache_header_present()
    {
      response.Headers["cache-control"].ShouldBe("private, max-age=3600");
    }

    [Fact]
    public void response_is_ok()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}