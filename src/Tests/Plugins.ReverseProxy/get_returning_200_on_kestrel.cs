using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Tests.Hosting.Owin;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
#if NETCOREAPP2_0
  public class get_returning_200_on_kestrel
  {
    [Fact(Skip = "#136 – fails, disable until https://github.com/dotnet/corefx/issues/28156 is resolved")]
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

  public class corefx_repro
  {
    [Fact]
    public async Task maybe_itll_fail_i_dont_even()
    {
      var toServer =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Configure(app => { app.Run(async context => await context.Response.WriteAsync("hello")); })
          .Build();
      toServer.Start();
      var toPort = toServer.Port();
      var reverseProxyClient = new HttpClient();
      var fromServer=new WebHostBuilder()
        .Configure(app =>
        {
          app.Run(async context =>
          {
            var proxiedResponse = await reverseProxyClient.GetAsync($"http://127.0.0.1:{toPort}/");
            context.Response.StatusCode = (int) proxiedResponse.StatusCode;
          });
        })
        .UseKestrel()
        .UseUrls("http://127.0.0.1:0")
        .Build();
      fromServer.Start();

      var client = new HttpClient();
      var response = await client.GetAsync($"http://127.0.0.1:{fromServer.Port()}");
      response.EnsureSuccessStatusCode();
    }
  }
#endif
  
}