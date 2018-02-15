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
    readonly HttpResponseMessage response;
    
    public get_returning_200()
    {
      response = new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied")
          .GetAsync("http://localhost/proxy")
          .Result;
    }

    [Fact]
    public async Task response_status_is_correct()
    {
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied");
    }
  }
}