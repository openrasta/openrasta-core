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
    readonly HttpResponseMessage response;

    public get_with_query_string()
    {
      response = new ProxyServer()
          .FromServer("/proxy-with-qs")
          .ToServer("/proxied")
          .GetAsync("http://localhost/proxy-with-qs?q=test")
          .Result;
    }


    [Fact]
    public void response_status_code_is_correct()
    {
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied?q=test");
    }
  }
}