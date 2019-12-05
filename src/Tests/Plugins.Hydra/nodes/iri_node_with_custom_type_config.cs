using System;
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
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class iri_node_with_custom_type_config : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;
    string responseContent;

    public iri_node_with_custom_type_config()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx =>
              ctx.Transient(() => new PreCompiledUtf8JsonHandler()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<Person>()
            .Vocabulary("https://schemas.example/schema#")
            .Type(datum => $"event-types/{datum.Id}")
            .AtUri("/events/{id}/organiser");
          
          ResourceSpace.Has.ResourcesOfType<EventWithPerson>()
            .Vocabulary("https://schemas.example/schema#")
            .Type(datum => $"CustomEvent")
            .AtUri("/events/{id}")
            .HandledBy<PersonHandler>();
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

    class PersonHandler
    {
      public EventWithPerson Get(int id) => new EventWithPerson() {Id = id, Organiser = new Person(){Id = 666}};
    }

    [Fact]
    public void type_is_overridden()
    {
      body["@type"].ShouldBe("http://localhost/CustomEvent");
      body["organiser"]["@type"].ShouldBe("http://localhost/event-types/666");
      body["organiser"]["@id"].ShouldBe("http://localhost/events/666/organiser");
    }

    public async Task InitializeAsync()
    {
      responseContent = await server.GetJsonLdString("/events/2");
      (response, body) = await server.GetJsonLd("/events/2");
    }

    public async Task DisposeAsync() => server.Close();
  }
}