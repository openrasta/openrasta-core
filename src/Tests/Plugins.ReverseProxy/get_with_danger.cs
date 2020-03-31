using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_danger : IDisposable
  {
    readonly HttpClient client;
    readonly IWebHost webHost;

    public get_with_danger()
    {
      this.webHost = new WebHostBuilder()
        .Configure(builder => builder.Run(ctx => ctx.Response.WriteAsync("Danger!")))
        .UseKestrel(options =>
        {
          options.Listen(IPAddress.Any,
            5050,
            listenOptions =>
            {
              var certificate = new X509Certificate2(Path.Combine(Directory.GetCurrentDirectory(), "Plugins.ReverseProxy", "testCert.pfx"), "testPassword");
              listenOptions.UseHttps(certificate);
            });
        }).Build();

      this.webHost.Start();

      this.client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
    }

    [Fact]
    public async Task test_name()
    {
      var response = await this.client.GetAsync("https://localhost:5050/");
      var content = await response.Content.ReadAsStringAsync();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("Danger!", content);
    }
    
    [Fact]
    public async Task test_name2() 
    {
      using (var response = await new ProxyServer()
        .FromServer(port => $"http://127.0.0.1:{port}/proxy")
        .ToServer(port => "https://google.com")
        .UseKestrel()
        .GetAsync("/"))
      {
        response.Message.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
      }
    }

    public void Dispose()
    {
      this.client?.Dispose();
      this.webHost?.Dispose();
    }
  }
}