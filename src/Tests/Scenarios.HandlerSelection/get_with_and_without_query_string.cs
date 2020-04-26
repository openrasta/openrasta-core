using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection
{
  public class get_with_and_without_query_string
  {
    InMemoryHost server;

    public get_with_and_without_query_string()
    {
      server = new TestHost((has,uses) =>
        has
          .ResourcesNamed("example")
          .AtUri("/account/")
          .And.AtUri("/account/?id={account_id}")
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