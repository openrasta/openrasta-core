using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Tests.Pipeline.Middleware.Conversion;

namespace Tests.Scenarios.HandlerSelection
{
  public class post_with_and_without_uri_var_as_string
  {
    InMemoryHost server;

    public post_with_and_without_uri_var_as_string()
    {
      server = new TestHost((has,uses) =>
        has
          .ResourcesNamed("example")
          .AtUri("/domain/new")
          .And.AtUri("/account/{account_id}/domain/new")
          .HandledBy<CollectionHandler<string,string>>());
    }
    
    [GitHubIssue(98)]
    public async Task post_with_uri_var_gets_selected()
    {
      var response = await server.Post("/account/1/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST:1");
    }
    
    [GitHubIssue(98)]
    public async Task post_with_no_uri_var_gets_selected()
    {
      var response = await server.Post("/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST");
    }
  }
}