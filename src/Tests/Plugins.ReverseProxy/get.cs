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

  public class post : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public post()
    {
      server = ProxyTestServer.Create();

      response = server
          .CreateRequest("/proxy")
          .PostAsync()
          .Result;
    }
    public void Dispose()
    {
      server.Dispose();
    }
  }

  public class get_returning_406 : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_returning_406()
    {
      server = ProxyTestServer.Create();

      response = server
          .CreateRequest("/proxy")
          .AddHeader("Accept", "application/vnd.example")
          .GetAsync()
          .Result;
    }

    [Fact]
    public void response_status_code_is_correct()
    {
      response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);
    }

    public void Dispose()
    {
      server.Dispose();
    }
  }

  public class get_returning_200 : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_returning_200()
    {
      server = ProxyTestServer.Create();

      response = server
          .CreateRequest("/proxy")
          .GetAsync()
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
      (await response.Content.ReadAsStringAsync()).ShouldBe("OK");
    }

    public void Dispose()
    {
      server.Dispose();
    }
  }
}