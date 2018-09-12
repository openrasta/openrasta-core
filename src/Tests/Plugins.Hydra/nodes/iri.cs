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

namespace Tests.Plugins.Hydra.nodes
{
  public class collection : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public collection()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options => options.Vocabulary = "https://schemas.example/schema#");

        ResourceSpace.Has.ResourcesOfType<List<Event>>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/")
          .HandledBy<EventHandler>();
        
        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{id}")
          .HandledBy<EventHandler>();
      });
    }

    [Fact]
    public void content_is_correct()
    {
      body["@type"].ShouldBe("hydra:Collection");
      body["totalItems"].ShouldBe(2);
      body["member"].ShouldBeOfType<JArray>();
      body["member"][0]["@id"].ShouldBe("http://localhost/events/1");
      body["member"][0]["@type"].ShouldBe("Event");
      body["member"][0]["id"].ShouldBe(1);
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }

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
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/2");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }

  public class EventHandler
  {
    public Event Get(int id)
    {
      return new Event { Id = id };
    }

    public List<Event> Get()
    {
      return new List<Event>
      {
        new Event() {Id = 1},
        new Event() {Id = 2}
      };
    }
  }
}