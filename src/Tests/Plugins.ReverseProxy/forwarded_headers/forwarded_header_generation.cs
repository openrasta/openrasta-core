using System;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class forwarded_header_parsing
  {
    [Fact]
    public void one_key_with_token_value_is_parsed()
    {
      var header = ForwardedHeader.Parse("key=value")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value");
    }
        
    [Fact]
    public void multiple_pairs_with_token_value()
    {
      var header = ForwardedHeader.Parse("key=value;another=anothervalue")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value");
      header["another"].ShouldBe("anothervalue");
    }
    [Fact]
    public void key_with_quoted_string_value()
    {
      var header = ForwardedHeader.Parse("key=\"value with space\"")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value with space");
    }
    [Fact]
    public void key_with_empty_quoted_string()
    {
      var header = ForwardedHeader.Parse("key=\"\"")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("");
    }
    [Fact]
    public void multiple_key_value()
    {
      var header = ForwardedHeader.Parse("key=one,key=two");
          
      header.ElementAt(0)["key"].ShouldBe("one");
      header.ElementAt(1)["key"].ShouldBe("two");
    }
  }
  public class forwarded_header_generation
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