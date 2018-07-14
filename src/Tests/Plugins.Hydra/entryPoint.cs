using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra
{
  public class entryPoint
  {
    InMemoryHost server;

    public entryPoint()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra();

        ResourceSpace.Has
          .ResourcesOfType<List<Event>>()
          .Vocabulary(ExampleVocabularies.Events)
          .AtUri("/events")
          .Collection();

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary(ExampleVocabularies.Events)
          .AtUri("/events/{id}");
      });
    }

    [Fact]
    public async Task hydra_identifiers_are_correct()
    {
      var response = await server.GetJsonLd("/");
      response["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
      response["@id"].ShouldBe("http://localhost/");
      response["@type"].ShouldBe("hydra:EntryPoint");
    }

    [Fact]
    public async Task collection_is_linked()
    {
      var response = await server.GetJsonLd("/");
      response["collection"].ShouldBeOfType<JArray>();
      response["collection"][0]["@type"].ShouldBe("hydra:Collection");
      response["collection"][0]["@id"].ShouldBe("http://localhost/events");
    }
  } 
}