using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.wf
{
  public class datum_is_valid : IAsyncLifetime
  {
    
    InMemoryHost server;
    IResponse response;
    JToken body;

    public datum_is_valid()
    {
      server = new InMemoryHost(new Api(), startup: new StartupProperties {OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
    }

    [Fact]
    public void json_is_valid()
    {
    }
    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/datum");
    }

    public async Task DisposeAsync() => server.Close();
  }
}