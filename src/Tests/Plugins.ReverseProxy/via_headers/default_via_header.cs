using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
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
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Via"])
        .GetAsync("http://source.example/proxy");
      response.Content.ShouldBe("HTTP/2.0 source.example:80");

      var via = response.Message.Headers.Via.ShouldHaveSingleItem();
      via.ReceivedBy.ShouldBe("source.example:80");
    }
  }
}