using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_segmemt_and_qs_vars
  {
    readonly HttpResponseMessage response;


    public get_with_segmemt_and_qs_vars()
    {
      response = new ProxyServer()
          .FromServer("/proxy/{first}/{second}/?q={third}")
          .ToServer("/proxied/{first}/{second}/?query={third}")
          .GetAsync("http://localhost/proxy/one/two/?q=three")
          .Result;
    }

    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied/one/two/?query=three");
    }
  }
}