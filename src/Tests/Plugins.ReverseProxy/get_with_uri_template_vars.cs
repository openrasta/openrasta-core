using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_uri_template_vars
  {
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy/{first}/{second}/")
          .ToServer("/proxied/{first}/{second}/")
          .GetAsync("http://localhost/proxy/one/two/");
      response.content.ShouldBe("http://destination.example/proxied/one/two/");
      response.dispose();
    }
  }
}