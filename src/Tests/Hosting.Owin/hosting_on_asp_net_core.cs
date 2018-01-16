using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Hosting.Katana;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
  public class hosting_on_asp_net_core
  {
    readonly HttpClient client;

    public hosting_on_asp_net_core()
    {
      var server = new TestServer(
        new WebHostBuilder()
          .Configure(app =>
            app.UseOwin(builder =>
              builder.UseOpenRasta(new TaskApi()))));
      client = server.CreateClient();
    }
    
    [Fact]
    public async void can_get_list_of_tasks()
    {
      var response = await client.GetAsync("/tasks");
      response.EnsureSuccessStatusCode();
    }
  }
}