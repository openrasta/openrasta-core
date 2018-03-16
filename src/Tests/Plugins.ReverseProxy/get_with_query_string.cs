using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_query_string
  {
    [Fact]
    public async Task response_status_and_uri_is_correct()
    {
      using (var response = await new ProxyServer().FromServer("/proxy-with-qs")
        .ToServer("/proxied")
        .GetAsync("http://source.example/proxy-with-qs?q=test"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("http://destination.example/proxied?q=test");
      }
    }
  }
}