using System;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class forwarded_header_generation
  {
    [Fact]
    public async Task legacy_is_rewritten()
    {
      var (_, content, dispose) = await new ProxyServer()
          .FromServer("/proxy", options => options.FrowardedHeaders.ConvertLegacyHeaders = true)
          .ToServer("/proxied", ctx => ctx.Request.Headers["X-Forwarded-Host"] + "|" + ctx.Request.Headers["Forwarded"])
          .AddHeader("X-Forwarded-Host", "openrasta.example")
          .AddHeader("X-Forwarded-Proto", "https")
          .GetAsync("/proxy");

      content.ShouldBe("|host=openrasta.example;proto=https,proto=http;host=localhost");
      dispose();
    }

    [Fact]
    public async Task forwarded_chain_is_preserved()
    {
      var (_, content, dispose) = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.Request.Headers["Forwarded"])
          .AddHeader("Forwarded", "host=openrasta.example")
          .GetAsync("/proxy");

      content.ShouldBe("host=openrasta.example,proto=http;host=localhost");
      dispose();
    }
  }
}