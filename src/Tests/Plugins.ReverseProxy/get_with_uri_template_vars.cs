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
  public class get_with_uri_template_vars : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_with_uri_template_vars()
    {
      server = ProxyTestServer.Create();

      response = server
          .CreateRequest("/proxy/test/")
          .GetAsync()
          .Result;
    }


    [Fact(Skip="Not implemented yet")]
    public void response_status_code_is_correct()
    {
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(Skip="Not implemented yet")]
    public async Task response_status_body_is_proxied()
    {
      (await response.Content.ReadAsStringAsync()).ShouldBe("test");
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}