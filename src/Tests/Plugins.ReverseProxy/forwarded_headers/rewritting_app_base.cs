using System.Threading.Tasks;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class rewritting_app_base
  {
    [Fact]
    public async Task enabled_header_with_base()
    {
      using (var response = await new ProxyServer()
        .FromServer("/base/proxy")
        .ToServer(
          "/base/proxied",
          async ctx => $"{ctx.ApplicationBaseUri.ToString()}|{ctx.Request.Uri}",
          options => options.FrowardedHeaders.RunAsForwardedHost = true,
          resourceRegistrationUri: "/proxied")
        .AddHeader("Forwarded", "host=openrasta.example;proto=https;base=\"/base\"")

        .GetAsync("base/proxy"))
      {
        // TODO: Can't rewrite the app base correctly due to ICommContext appbase  being readonly, needs fixing
        response.Content.ShouldBe("https://openrasta.example/|https://openrasta.example/proxied");
      }
    }
    [Fact]
    public async Task enabled_header()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async ctx => ctx.ApplicationBaseUri.ToString(),
          options => options.FrowardedHeaders.RunAsForwardedHost = true)
        .AddHeader("Forwarded", "host=openrasta.example;proto=https")
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe("https://openrasta.example/");
      }
    }

    [Fact]
    public async Task enabled_no_header()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async ctx => ctx.ApplicationBaseUri.ToString(),
          options => options.FrowardedHeaders.RunAsForwardedHost = true)
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe("http://localhost/");
      }
    }

    [Fact]
    public async Task disabled()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async ctx => ctx.ApplicationBaseUri.ToString(),
          options => options.FrowardedHeaders.RunAsForwardedHost = false)
        .AddHeader("Forwarded", "host=openrasta.example;proto=https")
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe("http://destination.example/");
      }
    }
  }
}