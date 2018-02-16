using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_uri_template_vars : IDisposable
  {
    readonly (HttpResponseMessage response, string content, Action dispose) response;

    public get_with_uri_template_vars()
    {
      response = new ProxyServer()
          .FromServer("/proxy/{first}/{second}/")
          .ToServer("/proxied/{first}/{second}/")
          .GetAsync("http://localhost/proxy/one/two/")
          .Result;
    }
    
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      response.content.ShouldBe("http://localhost/proxied/one/two/");
    }

    public void Dispose()
    {
      response.dispose();
    }
  }
}