using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.IO;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class post_data
  {
    [Fact]
    public async Task data_is_received()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", context => Encoding.UTF8.GetString(context.Request.Entity.Stream.ReadToEnd()))
        .AddHeader("Content-Type", MediaType.ApplicationXWwwFormUrlencoded.ToString())
        .PostAsync("http://localhost/proxy", "key=value");

      response.response.StatusCode.ShouldBe(HttpStatusCode.OK);
      response.content.ShouldBe("key=value");
      response.dispose();
    }
  }

  public class get_returning_200
  {
    [Fact]
    public async Task response_status_is_correct()
    {
      var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied")
        .GetAsync("http://localhost/proxy");

      response.response.StatusCode.ShouldBe(HttpStatusCode.OK);
      response.content.ShouldBe("http://destination.example/proxied");
      response.dispose();
    }
  }
}