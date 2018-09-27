using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
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
  public class collection_with_uri_override : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public collection_with_uri_override()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options =>
        {
          
          options.Vocabulary = "https://schemas.example/schema#";
        });

        ResourceSpace.Has
          .ResourcesOfType<List<Event>>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{location}/")
          .EntryPointCollection(options=>options.Uri = "/events/gb/");

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{location}/{id}");
      });
    }


    [Fact]
    public async Task collection_is_linked()
    {
      body["collection"].ShouldBeOfType<JArray>();
      body["collection"][0]["@type"].ShouldBe("hydra:Collection");
      body["collection"][0]["@id"].ShouldBe("http://localhost/events/gb/");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }
}