using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Scenarios.ContentNegotiation.Type
{
  public class accept_header_with_wildcard_codec
  {
    InMemoryHost server;

    public accept_header_with_wildcard_codec()
    {
      server = new InMemoryHost(() =>
        ResourceSpace.Has
          .ResourcesOfType<TaskItem>()
          .AtUri("/{id}")
          .HandledBy<TaskHandler>()
          .TranscodedBy<NullCodec>().ForMediaType("*/*"));
    }

    [Fact]
    public async Task negotiated_type_is_specific_mt()
    {
      var response = await server.ProcessRequestAsync(new InMemoryRequest
      {
        HttpMethod = "GET",
        Headers = {{"Accept", "application/json"}},
        Uri = new Uri("http://localhost/1")
      });
      response.Headers.ContentType.ShouldBe(MediaType.Json);
    }
  }
}