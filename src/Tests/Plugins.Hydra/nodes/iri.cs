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
        ResourceSpace.Uses.Hydra(options =>
        {
          options.Vocabulary = "https://schemas.example/schema#";
          options.Utf8Json = true;
        });

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

    public async Task DisposeAsync() => server.Close();
  }
}


/*

{
  "@context": "http://localhost/.hydra/context.jsonld",
  "@id": "http://localhost/events/2",
  "@type": "Event",
  "id": 2,
  "firstName": "Bilbo Baggins"
}


{
  "id": 2,
  "firstName": "Bilbo Baggins"
}

{
  "@context": "http://localhost/.hydra/context.jsonld",
  "@id": "http://localhost/events/2",
  "@type": "Event",
  "Id": 2,
  "FirstName": "Bilbo Baggins"
}



{
  "@context": "http://localhost/.hydra/context.jsonld",
  "@type": "hydra:Collection",
  "member": [
    {
      "@id": "http://localhost/events/1",
      "@type": "Event",
      "id": 1
    },
    {
      "@id": "http://localhost/events/2",
      "@type": "Event",
      "id": 2
    }
  ],
  "totalItems": 2
}


*/