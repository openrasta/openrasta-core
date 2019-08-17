using System.Collections;
using System.Collections.Generic;
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
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class custom_collection_type : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public custom_collection_type()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options =>
        {
          options.Vocabulary = "https://schemas.example/schema#";
          options.Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
        });

        ResourceSpace.Has.ResourcesOfType<EventCollection>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/")
          .HandledBy<Handler>();

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{id}");

        ResourceSpace.Has.ResourcesOfType<Customer>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri(customer => $"/customers/{customer.Name}");
        
      }, startup: new StartupProperties {OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
    }

    public class EventCollection : IEnumerable<Event>
    {
      List<Event> _events = new List<Event>();
      public string CustomProperty => "CustomPropertyValue";

      public IEnumerator<Event> GetEnumerator()
      {
        return _events.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
      public void Add(Event @event){
       _events.Add(@event);
    }
    
    }

    public class Handler
    {
      public EventCollection Get()
      {
        return new EventCollection
        {
          new Event {Id = 1, Customer = new Customer {Name = "Boromear"}},
          new Event
          {Id = 2, Customers =
          {
            new Customer {Name = "An elf"},
            new Customer {Name = "Another elf"},
          }}
        };
      }

    }

    [Fact]
    public void content_is_correct()
    {
      body["@type"].ShouldBe("EventCollection");
      body["totalItems"].ShouldBe(2);
      body["member"].ShouldBeOfType<JArray>();
      body["member"][0]["@id"].ShouldBe("http://localhost/events/1");
      body["member"][0]["@type"].ShouldBe("Event");
      body["member"][0]["id"].ShouldBe(1);
    }

//    [Fact]
    public void has_custom_properties()
    {
      body["@type"].ShouldBe("EventCollection");
      body["customProperty"].ShouldBe("CustomPropertyValue");
    }
    
    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/");
    }

    public async Task DisposeAsync() => server.Close();
  }
}