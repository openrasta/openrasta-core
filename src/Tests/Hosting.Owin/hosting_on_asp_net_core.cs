using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Hosting.AspNetCore;
using Shouldly;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
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
      var response = await client.GetAsync("tasks");
      response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async void can_get_silent_ping()
    {
      var response = await client.GetAsync("ping-silently");
      response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
      response.Content.Headers.TryGetValues("Content-Length", out _).ShouldBeFalse();
    }

    [Fact]
    public async void can_get_no_content_ping()
    {
      var response = await client.GetAsync("ping-empty-content");
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
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
}