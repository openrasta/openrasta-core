using System.Threading.Tasks;
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
      using (var response = await new ProxyServer()
        .FromServer("/proxy", options => options.FrowardedHeaders.ConvertLegacyHeaders = true)
        .ToServer("/proxied",
          async ctx => ctx.Request.Headers["X-Forwarded-Host"] + "|" + ctx.Request.Headers["Forwarded"])
        .AddHeader("X-Forwarded-Host", "openrasta.example")
        .AddHeader("X-Forwarded-Proto", "https")
        .GetAsync("/proxy"))

      {
        response.Content.ShouldBe("|host=openrasta.example;proto=https,proto=http;host=localhost");
      }
    }

    [Fact]
    public async Task forwarded_chain_is_preserved()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Forwarded"])
        .AddHeader("Forwarded", "host=openrasta.example")
        .GetAsync("/proxy"))
      {
        response.Content.ShouldBe("host=openrasta.example,proto=http;host=localhost");
      }
    }
  }
}