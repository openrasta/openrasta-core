using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy.errors
{
#if NET6_0_OR_GREATER
  public class unknown_dns
  {
    [Fact]
    public async Task response_is_bad_gateway()
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://127.0.0.1:{port}/proxy")
        .ToServer(port => $"http://unknown.example")
        .UseKestrel()
        .GetAsync("/proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
      }
    }
  }
#endif
}