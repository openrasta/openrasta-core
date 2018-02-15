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
  public class get_with_segmemt_qs_vars_and_unmapped_vars : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_with_segmemt_qs_vars_and_unmapped_vars()
    {
      server = ProxyTestServer.Create(
          "/proxy/{first}/{second}/?q={third}",
          "/proxied/{first}/{second}/?query={third}");

      response = server
          .CreateRequest("http://localhost/proxy/one/two/?q=three&another=fourth")
          .GetAsync()
          .Result;
    }
    
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied/one/two/?query=three&another=fourth");
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
  public class get_with_segmemt_and_qs_vars : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_with_segmemt_and_qs_vars()
    {
      server = ProxyTestServer.Create(
          "/proxy/{first}/{second}/?q={third}",
          "/proxied/{first}/{second}/?query={third}");

      response = server
          .CreateRequest("http://localhost/proxy/one/two/?q=three")
          .GetAsync()
          .Result;
    }
    
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied/one/two/?query=three");
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
  public class get_with_uri_template_vars : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_with_uri_template_vars()
    {
      server = ProxyTestServer.Create(
          "/proxy/{first}/{second}/",
          "/proxied/{first}/{second}/");

      response = server
          .CreateRequest("http://localhost/proxy/one/two/")
          .GetAsync()
          .Result;
    }
    
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("http://localhost/proxied/one/two/");
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}