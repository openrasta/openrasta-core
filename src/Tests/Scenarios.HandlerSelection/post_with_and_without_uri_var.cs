using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;

namespace Tests.Scenarios.HandlerSelection
{
  public abstract class post_with_and_without_uri_var<TKey>
  {
    InMemoryHost server;

    public post_with_and_without_uri_var()
    {
      server = new TestHost((has, uses) =>
        has
          .ResourcesNamed("example")
          .AtUri("/domain/new")
          .And.AtUri("/account/{account_id}/domain/new")
          .HandledBy<CollectionHandler<TKey, string>>());
    }

    [GitHubIssue(98)]
    public async Task post_with_uri_var_gets_selected()
    {
      var response = await server.Post("/account/1/domain/new", "my new stuff");
      response.ReadString().ShouldBe("POST:1:my new stuff");
    }

    [GitHubIssue(98)]
    public async Task post_with_no_uri_var_gets_selected()
    {
      var response = await server.Post("/domain/new", "everyone's new stuff");
      response.ReadString().ShouldBe("POST:everyone's new stuff");
    }
  }
}