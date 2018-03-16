using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_segmemt_and_qs_vars
  {
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      using (var response = await new ProxyServer().FromServer("/proxy/{first}/{second}/?q={third}")
        .ToServer("/proxied/{first}/{second}/?query={third}")
        .GetAsync("http://localhost/proxy/one/two/?q=three"))
      {
        response.Content.ShouldBe("http://destination.example/proxied/one/two/?query=three");
      }
    }
  }
}