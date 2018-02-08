using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using OpenRasta.Hosting.AspNetCore;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
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
      server.RunAsync();
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
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}