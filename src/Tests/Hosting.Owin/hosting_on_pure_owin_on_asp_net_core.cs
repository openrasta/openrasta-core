using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenRasta.Hosting.Katana;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
  public class hosting_on_pure_owin_on_asp_net_core : IDisposable
  {
    readonly HttpClient client;
    readonly TestServer server;

    public hosting_on_pure_owin_on_asp_net_core()
    {
      server = new TestServer(
        new WebHostBuilder()
          .Configure(app =>
            app.UseOwin(builder =>
              builder.UseOpenRasta(
                new TaskApi(),
                onAppDisposing: app.ApplicationServices.GetService<IApplicationLifetime>().ApplicationStopping))));
      client = server.CreateClient();
    }
    
    [Fact]
    public async void can_get_list_of_tasks()
    {
      var response = await client.GetAsync("/tasks");
      response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
      server?.Dispose();
    }
  }
}