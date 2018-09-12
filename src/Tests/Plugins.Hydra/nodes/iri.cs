using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class iri : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public iri()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options => options.Vocabulary = "https://schemas.example/schema#");

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{id}")
          .HandledBy<EventHandler>();
      });
    }


    [Fact]
    public void content_is_correct()
    {
      body["@id"].ShouldBe("http://localhost/events/2");
      body["@type"].ShouldBe("Event");
      body["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/2");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }
}