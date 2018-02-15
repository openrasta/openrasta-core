using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using OpenRasta.Hosting.AspNetCore;
using Shouldly;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
  #if NETCOREAPP2_0
  public class hosting_on_kestrel : IDisposable
  {
    
    readonly HttpClient client;
    readonly IWebHost server;
    static Random randomPort = new Random();

    public hosting_on_kestrel()
    {
      var port = randomPort.Next(2048,4096);
      server =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls($"http://localhost:{port}")
          .Configure(app => app.UseOpenRasta(new TaskApi()))
          .Build();
      server.Start();
      
      client = new HttpClient {BaseAddress = new Uri($"http://localhost:{port}")};
    }

    [Fact]
    public async void can_get_list_of_tasks()
    {
      var response = await client.GetAsync("/tasks");
      response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async void can_get_silent_ping()
    {
      var response = await client.GetAsync("/ping-silently");
      response.EnsureSuccessStatusCode();
     
      response.Content.Headers.TryGetValues("Content-Length", out _).ShouldBeFalse();
    }

    [Fact]
    public async void can_get_no_content_ping()
    {
      var response = await client.GetAsync("/ping-empty-content");
      response.EnsureSuccessStatusCode();
      response.Content.Headers.ContentLength.ShouldBe(0);
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
  #endif
}