using OpenRasta.Plugins.Caching.Pipeline;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_none_match
{
  public class not_matching : caching
  {
    public not_matching()
    {
      given_resource<TestResource>(map => map.Etag(_ => "v2"));
      given_request_header("if-none-match", Etag.StrongEtag("v1"));

      when_executing_request("/TestResource");
    }

    [Fact]
    public void returns_precondition_failed()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}