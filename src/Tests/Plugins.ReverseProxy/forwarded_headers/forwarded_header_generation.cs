using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OpenRasta.Collections;
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
        .FromServer("/proxy", options => options.ForwardedHeaders.ConvertLegacyHeaders = true, "10.0.0.1")
        .ToServer("/proxied",
          async ctx => ctx.Request.Headers["X-Forwarded-Host"] + "|" + ctx.Request.Headers["Forwarded"])
        .AddHeader("X-Forwarded-Host", "openrasta.example")
        .AddHeader("X-Forwarded-Proto", "https")
        .AddHeader("X-Forwarded-Base", "app")
        .GetAsync("proxy"))

      {
        response.Content.ShouldBe("|host=openrasta.example;proto=https;base=\"/app\";by=10.0.0.1,proto=http;host=localhost;by=10.0.0.1");
      }
    }

    [Fact]
    public async Task forwarded_chain_is_preserved()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy")
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Forwarded"])
        .AddHeader("Forwarded", "host=openrasta.example")
        .AddHeader("Forwarded", "host=openrasta.example2")
        .GetAsync("proxy"))
      {
        response.Content.ShouldStartWith("host=openrasta.example,host=openrasta.example2,proto=http;host=localhost;by=");
      }
    }
    
    [Fact]
    public async Task by_is_set_to_owin_local_ip_when_it_is_present()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy", localIpAddress: "10.0.10.1")
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Forwarded"])
        .AddHeader("Forwarded", "host=openrasta.example")
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe("host=openrasta.example,proto=http;host=localhost;by=10.0.10.1");
      }
    }
    
    [Fact]
    public async Task by_is_set_to_network_interface_ip_when_no_owin_local_ip_is_present_and_only_one_external_interface_exists()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy", networkIpAddresses: new List<IPAddress>
        {
          IPAddress.Parse("127.0.0.1"),
          IPAddress.Parse("10.0.0.2")
        })
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Forwarded"])
        .AddHeader("Forwarded", "host=openrasta.example")
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe("host=openrasta.example,proto=http;host=localhost;by=10.0.0.2");
      }
    }
    
    [Fact]
    public async Task by_is_set_to_obfuscated_machine_name_when_no_owin_local_ip_is_present_and_more_than_one_external_interface_exists()
    {
      using (var response = await new ProxyServer()
        .FromServer("/proxy", networkIpAddresses: new List<IPAddress>
        {
          IPAddress.Parse("10.0.0.2"),
          IPAddress.Parse("10.0.0.3")
        })
        .ToServer("/proxied", async ctx => ctx.Request.Headers["Forwarded"])
        .AddHeader("Forwarded", "host=openrasta.example")
        .GetAsync("proxy"))
      {
        response.Content.ShouldBe($"host=openrasta.example,proto=http;host=localhost;by=_{Environment.MachineName}");
      }
    }
  }
}