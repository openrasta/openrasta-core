using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class iri_node_named_resource_types : IAsyncLifetime
  {
    
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public iri_node_named_resource_types()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
          });

          ResourceSpace.Has.ResourcesOfType<Event>()
            .Named("MyEvents")
            .Vocabulary("https://schemas.example/schema#")
            .AtUri(ev=> $"/my-events/{ev.Id}")
            .HandledBy<EventHandler>();

          ResourceSpace.Has.ResourcesOfType<Event>()
            .Named("YourEvents")
            .Vocabulary("https://schemas.example/schema#")
            .AtUri(ev=> $"/your-events/{ev.Id}")
            .HandledBy<EventHandler>();

          
        },
        startup: new StartupProperties()
        {
          OpenRasta =
          {
            Diagnostics = {TracePipelineExecution = false},
            Errors = {HandleAllExceptions = false, HandleCatastrophicExceptions = false}
          }
        });
    }


    [Fact]
    public void correct_uri_is_selected()
    {
      body["@id"].ShouldBe("http://localhost/your-events/2");
      body["@type"].ShouldBe("Event");
      body["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/your-events/2");
    }

    public async Task DisposeAsync() => server.Close();
  }
}