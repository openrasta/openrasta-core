using System;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.last_modified
{
  public class not_mapped : caching
  {
    public not_mapped()
    {
      given_current_time(now);
      given_resource(
        "/resource",
        new ResourceWithLastModified {LastModified = now});

      when_executing_request("/resource");
    }

    [Fact]
    public void header_not_present()
    {
      response.Headers.ContainsKey("last-modified").ShouldBeFalse();
    }

    [Fact]
    public void request_successful()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}