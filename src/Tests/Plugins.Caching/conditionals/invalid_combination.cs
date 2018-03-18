using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Caching.Pipeline;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.conditionals
{
  public class invalid_combination : caching
  {
    public invalid_combination()
    {
      given_resource<TestResource>(map => map.Etag(_ => "v1"));
      given_request_header("if-match", Etag.StrongEtag("v1"));
      given_request_header("if-none-match", Etag.StrongEtag("v1"));
      when_executing_request("/TestResource");
    }

    [Fact]
    public void warning_header_generated()
    {
      response.Headers["warning"]
        .Split(separator: ',')
        .Any(_ => _.Trim().StartsWith("199 If-Lolcat")).ShouldBeTrue();
    }
  }
}