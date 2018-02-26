using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.via_headers
{
  public class default_via_header
  {
    [Fact]
    public async Task request_domain_and_port_is_used()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", ctx => ctx.Request.Headers["Via"])
        .GetAsync("http://source.example/proxy");
      response.content.ShouldBe("1.1 source.example:80");

      var via = response.response.Headers.Via.ShouldHaveSingleItem();
      via.ReceivedBy.ShouldBe("source.example:80");
    }
  }
  public class pseudonym_via_header_request_absent
  {
    [Fact]
    public async Task rp_is_added()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy", options=>options.Via.Pseudonym = "componentName")
        .ToServer("/proxied", ctx => ctx.Request.Headers["Via"])
        .GetAsync("/proxy");
      response.content.ShouldBe("1.1 componentName");
    }
  }
  public class pseudonym_via_header_request_present
  {
    [Fact]
    public async Task rp_is_appended()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy", options=>options.Via.Pseudonym = "componentName")
        .ToServer("/proxied", ctx => ctx.Request.Headers["Via"])
        .AddHeader("Via", "1.1 identifier")
        .GetAsync("/proxy");
      response.content.ShouldBe("1.1 identifier,1.1 componentName");
    }
  }
}