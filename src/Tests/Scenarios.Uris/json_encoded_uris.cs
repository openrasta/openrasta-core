using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Tests.Scenarios.HandlerSelection;
using Xunit;

namespace Tests.Scenarios.Uris
{
  public class json_encoded_uris
  {
    [Fact]
    public async Task are_processed_correctly()
    {
      var response = await (new InMemoryHost(() => ResourceSpace.Has
          .ResourcesNamed("json")
          .AtUri("/json?data={data}")
          .HandledBy<IdentityHandler>())
        .Get("/json?data=%7B+%22Name%22+%3A+%22pete%22+%7D"));
      response.ReadString().ShouldBe("{ \"Name\" : \"pete\" }");
    }

    public class IdentityHandler
    {
      public string Get(string data)
      {
        return data;
      }
    }
  }
}