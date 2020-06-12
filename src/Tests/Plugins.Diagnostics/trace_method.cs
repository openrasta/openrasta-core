using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.IO;
using OpenRasta.Plugins.Diagnostics;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Plugins.Diagnostics
{
  public class trace_method
  {
    [Fact]
    public async Task request_is_returned()
    {
      using (var server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesNamed("resource")
          .AtUri("/{id}").HandledBy<Handler>();
        ResourceSpace.Uses.Diagnostics(new DiagnosticsOptions { TraceMethod = true });
      }))
      {
        var response = await server.ProcessRequestAsync(new InMemoryRequest()
        {
          HttpMethod = "TRACE",
          Uri = new Uri("http://localhost/1"),
          Headers =
          {
            {"Accept", "*/*"},
            {"User-Agent", "stuff"}
          }
        });

        response.StatusCode.ShouldBe(200);
        response.Entity.ContentType?.MediaType.ShouldBe("message/http");
        using (var reader = new StreamReader(response.Entity.Stream, Encoding.UTF8))
          reader.ReadToEnd().ShouldBe(
            "TRACE /1 HTTP/1.1\r\n" +
            "Host: http://localhost/\r\n" +
            "Accept: */*\r\n" +
            "User-Agent: stuff\r\n\r\n");
      }
    }
  }

  public class Handler
  {
    public void Get()
    {
    }
  }
}