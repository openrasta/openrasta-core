using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_query_string : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_with_query_string()
    {
      server = ProxyTestServer.Create("/proxy-with-qs", "/proxied");

      response = server
          .CreateRequest("http://localhost/proxy-with-qs?q=test")
          .GetAsync()
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

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}