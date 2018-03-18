using OpenRasta.Configuration;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.etag
{
  public class valid : caching
  {
    public valid()
    {
      given_resource<TestResource>(map => map.Etag(_ => "v1"));

      when_executing_request("/TestResource");
    }

    [Fact]
    public void etag_present()
    {
      response.ShouldHaveVariantEtag("v1");
    }

    [Fact]
    public void request_successful()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}