using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.wf
{
  public class sitemap_is_valid : IAsyncLifetime
  {
    InMemoryHost server;
    IResponse response;
    JToken body;

    public sitemap_is_valid()
    {
      server = new InMemoryHost(new Api(), startup: new StartupProperties {OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
    }
    
    [Fact]
    public void json_is_valid()
    {
      body["name"].Value<string>().ShouldBe("Geography");
      body["hasPart"][0]["name"].Value<string>().ShouldBe("Address");
      body["hasPart"][0]["hasPart"][0]["@id"].Value<string>().ShouldEndWith("/Property Address Key");
    }
    
    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/sitemap/test");
    }

    public async Task DisposeAsync() => server.Close();
  }
}