using OpenRasta.Plugins.Caching.Configuration;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
  public class invalid_value : caching
  {
    public invalid_value()
    {
      given_current_time(now);
      given_resource<TestResource>(map => map.LastModified(_ => now - 1.Minutes()));
      given_request_header("if-modified-since", "not-a-date");

      when_executing_request("/TestResource");
    }

    [Fact]
    public void conditional_ignored()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}