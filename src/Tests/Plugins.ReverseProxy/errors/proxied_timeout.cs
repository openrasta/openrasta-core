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
    [Fact(Skip = "#136 – fails, disable until https://github.com/dotnet/corefx/issues/28156 is resolved")]
    public async Task response_is_gateway_timeout()
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://127.0.0.1:{port}/proxy", options => options.Timeout = TimeSpan.FromSeconds(30))
        .ToServer(port => $"http://127.0.0.1:{port}/proxied", async ctx =>
        {
          await Task.Delay(TimeSpan.FromSeconds(1));
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