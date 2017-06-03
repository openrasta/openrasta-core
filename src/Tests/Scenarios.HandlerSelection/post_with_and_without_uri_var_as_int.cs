using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection
{
  public class post_with_and_without_uri_var_as_int
  {
    InMemoryHost server;

    public post_with_and_without_uri_var_as_int()
    {
      server = new InMemoryHost(() =>
        ResourceSpace.Has
          .ResourcesNamed("example")
          .AtUri("/domain/new")
          .And.AtUri("/account/{account_id}/domain/new")
          .HandledBy<CollectionHandler<int,string>>());
    }

    [Fact]
    public async Task post_with_uri_var_gets_selected()
    {
      var response = await server.Post("/account/1/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST:1");
    }
    
    [Fact]
    public async Task post_with_no_uri_var_gets_selected()
    {
      var response = await server.Post("/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST");
    }
  }
}