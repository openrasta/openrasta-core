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
using Tests.Plugins.Hydra.Examples;
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
              ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<Person>()
            .Vocabulary("https://schemas.example/schema#")
            .Type(datum => $"custom/type/{datum.Id}")
            .AtUri("/data/{id}")
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

    class Handler
    {
      public Person Get(int id) => new Person() {Id = id};
    }

    [Fact]
    public void type_is_overridden()
    {
      responseContent.LastIndexOf("@type").ShouldBe(responseContent.IndexOf("@type"));
      body["@type"].ShouldBe("http://localhost/custom/type/2");
    }

    public async Task InitializeAsync()
    {
      responseContent = await server.GetJsonLdString("/data/2");
      (response, body) = await server.GetJsonLd("/data/2");
    }

    public async Task DisposeAsync() => server.Close();
  }
}