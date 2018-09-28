using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Tests.Plugins.Hydra.Utf8Json;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class compatibility_json_attributes : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public compatibility_json_attributes()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx =>
              ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<EventWithAttributes>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/stuff/{id}")
            .HandledBy<EventWithIgnoredPropertyHandler>();
        });
    }


    [Fact]
    public void json_ignore()
    {
      body.OfType<JProperty>().ShouldNotContain(p => p.Name == "stuff");
    }

    [Fact]
    public void json_property()
    {
      body["propertyNameByEmbeddedAttribute"].ShouldBe(true);
    }

    [Fact]
    public void newtonsoft_json_property()
    {
      body["propertyNameByJsonAttribute"].ShouldBe(true);
    }
    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/stuff/2");
    }

    public async Task DisposeAsync() => server.Close();

    class EventWithIgnoredPropertyHandler
    {
      public EventWithAttributes Get(int id) => new EventWithAttributes() {Id = id};
    }

    class EventWithAttributes
    {
      [JsonIgnore]
      public string Stuff => "Hello";
      
      public int Id { get; set; }

      [JsonProperty("propertyNameByEmbeddedAttribute")]
      public bool Property1 => true;
      [Newtonsoft.Json.JsonProperty("propertyNameByJsonAttribute")]
      public bool Property2 => true;
    }

    class JsonIgnoreAttribute : Attribute
    {
    }
    
    class JsonPropertyAttribute : Attribute
    {
      public string property_name { get; }

      public JsonPropertyAttribute(string propertyName)
      {
        property_name = propertyName;
      }
    }
  }

  public class iri_node : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public iri_node()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx =>
              ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<Event>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/events/{id}")
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