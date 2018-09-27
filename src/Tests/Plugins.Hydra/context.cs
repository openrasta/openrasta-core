using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
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
  public class context : IAsyncLifetime
  {
    readonly InMemoryHost server;
    IResponse response;
    JToken body;

    public context()
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
          .AtUri("/events")
          .EntryPointCollection();

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{id}");
      });
    }

    [Fact]
    public void system_vocabulary_curies_are_defined()
    {
      body.Count().ShouldBe(1);

      var context = body["@context"];

      context["hydra"].ShouldBe("http://www.w3.org/ns/hydra/core#");
      context["rdf"].ShouldBe("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
      context["xsd"].ShouldBe("http://www.w3.org/2001/XMLSchema#");
      context["schema"].ShouldBe("http://schema.org/");
    }

    [Fact]
    public void default_vocab_is_set()
    {
      body["@context"]["@vocab"].ShouldBe("https://schemas.example/schema#");
    }

    [Fact]
    public void each_class_term_has_new_vocab()
    {
      body["@context"]["Event"]["@context"]["@vocab"].ShouldBe("https://schemas.example/schema#Event/");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/.hydra/context.jsonld");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }
}