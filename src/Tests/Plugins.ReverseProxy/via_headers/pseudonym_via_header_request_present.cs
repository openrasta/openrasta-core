using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy.via_headers
{
  public class pseudonym_via_header_request_present
  {
    [Fact]
    public async Task rp_is_appended()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy", options=>options.Via.Pseudonym = "componentName")
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Via"])
        .AddHeader("Via", "1.1 identifier")
        .GetAsync("proxy");
      response.Content.ShouldBe("1.1 identifier,HTTP/2.0 componentName");
    }
  }
}