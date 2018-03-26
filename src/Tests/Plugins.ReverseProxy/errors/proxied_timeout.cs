using System;
using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy.errors
{
#if NETCOREAPP2_0
  public class proxied_timeout
  {
    [Fact]
    public async Task response_is_gateway_timeout()
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://127.0.0.1:{port}/proxy", options => options.Timeout = TimeSpan.FromSeconds(1))
        .ToServer(port => $"http://127.0.0.1:{port}/proxied", async ctx =>
        {
          await Task.Delay(TimeSpan.FromSeconds(30));
          return "OK";
        })
        .UseKestrel()
        .GetAsync("/proxy"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.GatewayTimeout);
      }
    }
  }

#endif
}