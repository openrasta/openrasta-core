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
  public class post
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public post()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options =>
        {
          options.Vocabulary = "https://schemas.example/schema#";
          options.Utf8Json = true;
        });
        
        ResourceSpace.Has.ResourcesOfType<CreateAction>().Vocabulary(Vocabularies.SchemaDotOrg);

        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/")
          .HandledBy<EventHandler>();
      });
    }


    [Fact]
    public async Task can_buy()
    {
      (response, body) = await server.PostJsonLd("/events/", @"
{
  ""@type"":""schema:CreateAction""
}
");
      body["@id"].ShouldBe("http://localhost/events/");
      body["@type"].ShouldBe("Event");
    }
  }
}