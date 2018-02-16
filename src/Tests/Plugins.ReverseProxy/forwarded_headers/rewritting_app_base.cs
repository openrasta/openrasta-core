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
          .ToServer("/proxied", ctx => ctx.ApplicationBaseUri.ToString(), options => options.FrowardedHeaders.RunAsForwardedHost = true)
          .AddHeader("Forwarded", "host=openrasta.example;proto=https")
          .GetAsync("/proxy");

      response.content.ShouldBe("https://openrasta.example/");
      response.dispose();
    }

    [Fact]
    public async Task no_header()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.ApplicationBaseUri.ToString(), options => options.FrowardedHeaders.RunAsForwardedHost = true)
          
          .GetAsync("/proxy");

      response.content.ShouldBe("http://localhost/");
      response.dispose();
    }
    [Fact]
    public async Task when_disabled_app_base_is_rewritten()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy")
          .ToServer("/proxied", ctx => ctx.ApplicationBaseUri.ToString(), options => options.FrowardedHeaders.RunAsForwardedHost = false)
          .AddHeader("Forwarded", "host=openrasta.example;proto=https")
          .GetAsync("/proxy");

      response.content.ShouldBe("http://localhost/");
      response.dispose();
    }
  }
}