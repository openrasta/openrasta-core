using System.Collections.Generic;
using System.Linq;
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

namespace Tests.Plugins.Hydra
{
  public class context : IAsyncLifetime
  {
    readonly InMemoryHost server;
    IResponse response;
    JToken body;

    public context()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra();

        ResourceSpace.Has
          .ResourcesOfType<List<Event>>()
          .Vocabulary(ExampleVocabularies.ExampleApp)
          .AtUri("/events")
          .Collection();

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary(Vocabularies.SchemaDotOrg)
          .AtUri("/events/{id}");
      });
    }

    [Fact]
    public void vocabularies_are_defined()
    {
      body.Count().ShouldBe(1);

      body["@context"]["hydra"].ShouldBe("http://www.w3.org/ns/hydra/core#");
      body["@context"]["rdf"].ShouldBe("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
      body["@context"]["xsd"].ShouldBe("http://www.w3.org/2001/XMLSchema#");
      body["@context"]["schema"].ShouldBe("http://schema.org/");
      body["@context"]["ev"].ShouldBe("https://openrasta.example/schemas/events#");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("http://localhost/.hydra/context.jsonld");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }
}