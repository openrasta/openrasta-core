using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class rewritting_app_base
  {
    [Fact]
    public async Task when_enabled_app_base_is_rewritten()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.ApplicationBaseUri.ToString(), options=>options.FrowardedHeaders.RunAsForwardedHost = true)
          .AddHeader("Forwarded", "host=openrasta.example;proto=https")
          .GetAsync("/proxy");

      (await response.Content.ReadAsStringAsync())
          .ShouldBe("https://openrasta.example/");
    }
    [Fact]
    public async Task when_disabled_app_base_is_rewritten()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.ApplicationBaseUri.ToString(), options=>options.FrowardedHeaders.RunAsForwardedHost = false)
          .AddHeader("Forwarded", "host=openrasta.example;proto=https")
          .GetAsync("/proxy");

      (await response.Content.ReadAsStringAsync())
          .ShouldBe("http://localhost/");
    }
  }
}