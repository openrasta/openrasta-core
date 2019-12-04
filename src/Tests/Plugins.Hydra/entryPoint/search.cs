using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra
{
  public class search : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public search()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options =>
        {
          options.Vocabulary = "https://schemas.example/schema#";
          options.Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonHandler()).As<IMetaModelHandler>()  ;
        });

        ResourceSpace.Has
          .ResourcesOfType<List<Event>>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/")
          .EntryPointCollection(options =>
          {
            options.Search = new IriTemplate("/events/{?q}")
            {
              Mapping =
              {
                new IriTemplateMapping("q")
              }
            };
          });

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{location}/{id}");
      }, startup: new StartupProperties(){OpenRasta = { Errors = {  HandleAllExceptions = false,HandleCatastrophicExceptions = false}}});
    }


    [Fact]
    public void search_link_is_added()
    {
      body["collection"].ShouldBeOfType<JArray>();
      var collection = body["collection"][0];

      collection["@type"].ShouldBe("hydra:Collection");
      collection["@id"].ShouldBe("http://localhost/events/");

      collection["search"]["@type"].ShouldBe("hydra:IriTemplate");
      collection["search"]["template"].ShouldBe("/events/{?q}");


      collection["search"]["mapping"][0]["variable"].ShouldBe("q");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/");
    }

    public Task DisposeAsync() => Task.CompletedTask;
  }
}