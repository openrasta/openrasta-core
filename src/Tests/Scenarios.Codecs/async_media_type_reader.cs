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
  public class async_media_type_reader
  {
    [Fact]
    public async Task is_processed()
    {
      var server = new InMemoryHost(() => ResourceSpace.Has
        .ResourcesOfType<DemoResource>()
        .AtUri("/demo")
        .HandledBy<DemoResourceHandler>()
        .TranscodedBy<DemoResourceReader>().ForMediaType("*/*"));

      var response = await server.Post("/demo", "data");
      response.StatusCode.ShouldBe(204);
    }
    class DemoResource {}
    class DemoResourceHandler { public void Post(DemoResource resource){}}

    class DemoResourceReader : IMediaTypeReaderAsync
    {
      public object Configuration { get; set; }
      public Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
      {
        return Task.FromResult<object>(new DemoResource());
      }
    }
  }
}