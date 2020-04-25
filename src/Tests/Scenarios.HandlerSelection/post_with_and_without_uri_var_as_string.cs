using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Tests.Pipeline.Middleware.Conversion;
using Xunit;

namespace Tests.Scenarios.HandlerSelection
{
  public class post_with_and_without_uri_var_as_string
  {
    InMemoryHost server;

    public post_with_and_without_uri_var_as_string()
    {
      server = new InMemoryHost(() =>
        ResourceSpace.Has
          .ResourcesNamed("example")
          .AtUri("/domain/new")
          .And.AtUri("/account/{account_id}/domain/new")
          .HandledBy<CollectionHandler<string,string>>());
    }

    [Fact]
    public async Task post_with_uri_var_gets_selected()
    {
      var response = await server.Post("/account/1/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST:1");
    }
    
    [Fact(Skip="Issue #98")]
    public async Task post_with_no_uri_var_gets_selected()
    {
      var response = await server.Post("/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST");
    }
  }
  public class get_with_and_without_query_string
  {
    InMemoryHost server;

    public get_with_and_without_query_string()
    {
      server = new TestHost((has,uses) =>
        has
          .ResourcesNamed("example")
          .AtUri("/account/")
          .And.AtUri("/account/?id={account_id")
          .HandledBy<CollectionHandler<string,string>>());
    }

    [Fact]
    public async Task get_without_qs_gets_Selected()
    {
      var response = await server.Get("/account/");
      response.ReadString().ShouldBe("GET");
    }
    
    [Fact]
    public async Task get_with_qs_gets_selected()
    {
      var response = await server.Get("/account/?id=1");
      response.ReadString().ShouldBe("GET:1");
    }
  }
}