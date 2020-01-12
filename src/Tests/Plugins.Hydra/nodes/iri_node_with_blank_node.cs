using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class EventWithPerson
  {
    public string Name { get; set; }

    public Person Organiser { get; set; }
    public int Id { get; set; }
    public List<Person> Attendees { get; set; }
  }

  public class iri_node_with_blank_node : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public iri_node_with_blank_node()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
          });

          ResourceSpace.Has.ResourcesOfType<Person>()
            .Vocabulary("https://schemas.example/schema#");
          
          ResourceSpace.Has.ResourcesOfType<EventWithPerson>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/events/{id}")
            .HandledBy<Handler>();
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
    public void inner_node_has_type()
    {
      body["@id"].ShouldBe("http://localhost/events/2");
      body["@type"].ShouldBe("EventWithPerson");
      body["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
      body["organiser"]["name"].ShouldBe("person name");
      body["organiser"]["@type"].ShouldBe("Person");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/2");
    }

    class Handler
    {
      public EventWithPerson Get(int id)
      {
        return new EventWithPerson()
        {
          Id = id, Name = "event",
          Organiser = new Person() {Name = "person name"}
        };
      }
    }

    public async Task DisposeAsync() => server.Close();
  }

  public class Person
  {
    public string Name { get; set; }
    public int Id { get; set; }
  }
}