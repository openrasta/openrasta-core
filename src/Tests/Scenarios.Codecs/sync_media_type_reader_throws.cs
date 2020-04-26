using System;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class sync_media_type_reader_throws
  {
    [GitHubIssue(8)]
    public async Task response_is_400()
    {
      var server = new InMemoryHost(() => ResourceSpace.Has
        .ResourcesOfType<DemoResource>()
        .AtUri("/demo")
        .HandledBy<DemoHandler>()
        .TranscodedBy<ThrowingCodec>().ForMediaType("*/*")
      );
      var response = await server.Post("/demo", "yo");
      response.StatusCode.ShouldBe(400);
    }
    class DemoResource { }
    class DemoHandler{ public void Post(DemoResource res)
    {
    }}
    class ThrowingCodec : IMediaTypeReader
    {
      public object Configuration { get; set; }
      public object ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
      {
        throw new InvalidOperationException("Something went wrong");
      }
    }
  }
}