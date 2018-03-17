using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
#if NETCOREAPP2_0
  public class get_returning_200_on_kestrel
  {
    [Fact]
    public async Task response_status_is_correct()
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://localhost:{port}/proxy")
        .ToServer(port => $"http://localhost:{port}/proxied")
        .UseKestrel()
        .GetAsync("proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldEndWith("/proxied");
      }
    }
  }
#endif
}