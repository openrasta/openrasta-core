using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

    public hosting_on_kestrel()
    {
      server =
        new WebHostBuilder()
          .UseKestrel()
          .UseUrls("http://127.0.0.1:0")
          .Configure(app => { app.UseOpenRasta(new TaskApi()); })
          .Build();

      server.Start();
      var port = server.Port();
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

    [Fact]
    public async Task can_read_aspnet_core_user_in_openrasta()
    {
      //Header value is Foo:Bar
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "Rm9vOkJhcg==");

      var response = await client.GetAsync("/authedtasks");
      var content = await response.Content.ReadAsStringAsync();

      content.ShouldBe("Foo");
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
#endif
}