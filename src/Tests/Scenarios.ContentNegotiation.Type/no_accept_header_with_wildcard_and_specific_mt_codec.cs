using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Scenarios.ContentNegotiation.Type
{
  public class no_accept_header_with_wildcard_and_specific_mt_codec
  {
    InMemoryHost server;

    public no_accept_header_with_wildcard_and_specific_mt_codec()
    {
      server = new InMemoryHost(() =>
        ResourceSpace.Has
          .ResourcesOfType<TaskItem>()
          .AtUri("/{id}")
          .HandledBy<TaskHandler>()
          .TranscodedBy<NullCodec>().ForMediaType("*/*").ForMediaType("application/json"));
    }

    [Fact]
    public async Task negotiated_type_is_specific_mt()
    {
      var response = await server.Get("/1");
      response.Headers.ContentType.ShouldBe(MediaType.Json);
    }
  }
}