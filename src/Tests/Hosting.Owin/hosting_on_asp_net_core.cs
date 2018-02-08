using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenRasta.Hosting.AspNetCore;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
  public class hosting_on_kestrel : IDisposable
  {
    
    readonly HttpClient client;
    readonly IWebHost server;

    public hosting_on_kestrel()
    {
      server =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://localhost:5050")
          .Configure(app => app.UseOpenRasta(new TaskApi()))
          .Build();
      server.Run();
      client = new HttpClient(){BaseAddress = new Uri("http://localhost:5050")};
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
  public class hosting_on_asp_net_core : IDisposable
  {
    readonly HttpClient client;
    readonly TestServer server;

    public hosting_on_asp_net_core()
    {
      server = new TestServer(
        new WebHostBuilder()
          .Configure(app => app.UseOpenRasta(new TaskApi())));
      client = server.CreateClient();
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
  public class hosting_on_asp_net_core_in_map : IDisposable
  {
    readonly HttpClient client;
    readonly TestServer server;

    public hosting_on_asp_net_core_in_map()
    {
      server = new TestServer(
        new WebHostBuilder()
          .Configure(app => app.Map("/api", api=>api.UseOpenRasta(new TaskApi()))));
      client = server.CreateClient();
    }

    [Fact]
    public async void can_get_list_of_tasks()
    {
      var response = await client.GetAsync("/api/tasks");
      response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}