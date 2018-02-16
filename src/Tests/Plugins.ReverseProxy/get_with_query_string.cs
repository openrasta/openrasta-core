using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_query_string
  {
    [Fact]
    public async Task response_status_code_is_correct()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy-with-qs")
          .ToServer("/proxied")
          .GetAsync("http://localhost/proxy-with-qs?q=test");

      response.response.StatusCode.ShouldBe(HttpStatusCode.OK);
      response.dispose();
    }

    [Fact]
    public async Task response_status_body_is_proxied()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy-with-qs")
          .ToServer("/proxied")
          .GetAsync("http://localhost/proxy-with-qs?q=test");

      response.content.ShouldBe("http://localhost/proxied?q=test");
      response.dispose();
    }
  }
}