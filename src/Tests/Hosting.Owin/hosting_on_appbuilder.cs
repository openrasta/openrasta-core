using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Owin.Testing;
using OpenRasta.Hosting.Katana;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Hosting.Owin
{
  // ReSharper disable once InconsistentNaming
  public class hosting_on_IAppBuilder
  {
    readonly HttpClient client;

    public hosting_on_IAppBuilder()
    {
      var server = TestServer.Create(app => app.UseOpenRasta(new TaskApi()));
      client = server.HttpClient;
      client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
    }

    [Fact]
    public async void can_get_list_of_tasks()
    {
      var response = await client.GetAsync("/tasks");
      response.EnsureSuccessStatusCode();
    }
  }
}