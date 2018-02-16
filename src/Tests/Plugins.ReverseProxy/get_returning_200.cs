using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_returning_200
  {
    readonly (HttpResponseMessage response, string content, Action dispose) response;

    [Fact]
    public async Task response_status_is_correct()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied")
          .GetAsync("http://localhost/proxy");
      response.response.StatusCode.ShouldBe(HttpStatusCode.OK);
      response.content.ShouldBe("http://localhost/proxied");
      response.dispose();
    }
  }
}