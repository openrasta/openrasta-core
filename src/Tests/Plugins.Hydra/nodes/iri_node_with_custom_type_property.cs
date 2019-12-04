using System.Threading.Tasks;
using Newtonsoft.Json;
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

  public class Datum
  {
    [JsonProperty("@id")]
    public string Id { get; set; }
    [JsonProperty("@type")]
    public string Type { get; set; }
  }

  public class DatumHandler
  {
    public Datum Get(int id)
    {
      return new Datum {Type = "https://fuck.skype/it/sucks", Id = id.ToString()};
    }
  }

  public class iri_node_with_custom_type_property : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;
    string responseContent;

    public iri_node_with_custom_type_property()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx =>
              ctx.Transient(() => new PreCompiledUtf8JsonHandler()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<Datum>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/data/{id}")
            .HandledBy<DatumHandler>();

          ResourceSpace.Has.ResourcesOfType<Customer>();
          
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
    public void id_is_overriden()
    {
      responseContent.LastIndexOf("@id").ShouldBe(responseContent.IndexOf("@id"));
      body["@id"].ShouldBe("2");
    }
    [Fact]
    public void type_is_overridden()
    {
      responseContent.LastIndexOf("@type").ShouldBe(responseContent.IndexOf("@type"));
      body["@type"].ShouldBe("https://fuck.skype/it/sucks");
    }

    public async Task InitializeAsync()
    {
      responseContent = await server.GetJsonLdString("/data/2");
      (response,body) = await server.GetJsonLd("/data/2");
    }

    public async Task DisposeAsync() => server.Close();
  }
}