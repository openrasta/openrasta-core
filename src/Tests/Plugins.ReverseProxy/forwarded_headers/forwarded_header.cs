using System;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class forwarded_header
  {
    [Fact]
    public async Task legacy_is_rewritten()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy", options => options.FrowardedHeaders.ConvertLegacyHeaders = true)
          .ToServer("/proxied", ctx => ctx.Request.Headers["X-Forwarded-Host"] + "|" + ctx.Request.Headers["Forwarded"])
          .AddHeader("X-Forwarded-Host", "openrasta.example")
          .AddHeader("X-Forwarded-Proto", "https")
          .GetAsync("/proxy");

      (await response.Content.ReadAsStringAsync())
          .ShouldBe("|host=openrasta.example;proto=https,proto=http;host=localhost");
    }

    [Fact]
    public async Task forwarded_chain_is_preserved()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.Request.Headers["Forwarded"])
          .AddHeader("Forwarded", "host=openrasta.example")
          .GetAsync("/proxy");

      (await response.Content.ReadAsStringAsync()).ShouldBe("host=openrasta.example,proto=http;host=localhost");
    }
  }
}