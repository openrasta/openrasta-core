using System.Collections.Generic;
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
  public class entryPoint : IAsyncLifetime
  {
    readonly InMemoryHost server;
    IResponse response;
    JToken body;

    public entryPoint()
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
    public void hydra_identifiers_are_correct()
    {
      body["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
      body["@id"].ShouldBe("http://localhost/");
      body["@type"].ShouldBe("hydra:EntryPoint");
    }

    [Fact]
    public void collection_is_linked()
    {
      body["collection"].ShouldBeOfType<JArray>();
      body["collection"][0]["@type"].ShouldBe("hydra:Collection");
      body["collection"][0]["@id"].ShouldBe("http://localhost/events");
    }

    [Fact]
    public void collection_item_type_is_correct()
    {
      var evCollection = body["collection"][0];
      evCollection["manages"]["property"].Value<string>().ShouldBe("rdf:type");
      evCollection["manages"]["object"].Value<string>().ShouldBe("schema:Event");
    }
    
    [Fact]
    public void api_document_link_header_is_present()
    {
      response.Headers["Link"]
        .ShouldContain("<http://localhost/.hydra/documentation.jsonld>; rel=\"http://www.w3.org/ns/hydra/core#apiDocumentation\"");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  } 
}