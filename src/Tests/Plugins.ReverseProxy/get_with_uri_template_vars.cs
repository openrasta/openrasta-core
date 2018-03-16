using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_uri_template_vars
  {
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy/{first}/{second}/")
        .ToServer("/proxied/{first}/{second}/")
        .GetAsync("http://localhost/proxy/one/two/"))
      {
        response.Content.ShouldBe("http://destination.example/proxied/one/two/");
      }
    }
  }
}