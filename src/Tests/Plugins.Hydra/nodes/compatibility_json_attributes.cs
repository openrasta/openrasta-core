using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
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
}