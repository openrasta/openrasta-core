using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;
using OpenRasta.Web;
using Shouldly;
using Tests.Pipeline.Middleware.Conversion;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.wf
{
  public class is_valid : IAsyncLifetime
  {
    InMemoryHost server;
    IResponse response;
    JToken body;

    public is_valid()
    {
      server = new InMemoryHost(new Api(), startup: new StartupProperties {OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
    }
    
//    [Fact]
//    public void json_is_valid()
//    {
//      body["member"][0]["group"]["key"].Value<int>().ShouldBe(0);
//    }
    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/schema");
    }

    public async Task DisposeAsync() => server.Close();
  }
}