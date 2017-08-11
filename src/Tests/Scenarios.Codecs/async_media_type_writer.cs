using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.IO;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class async_media_type_writer
  {
    [Fact]
    public async Task is_processed()
    {
      var server = new InMemoryHost(() => ResourceSpace.Has
        .ResourcesOfType<DemoResource>()
        .AtUri("/demo")
        .HandledBy<DemoResourceHandler>()
        .TranscodedBy<DemoResourceWriter>().ForMediaType("*/*"));

      var response = await server.Get("/demo");
      response.StatusCode.ShouldBe(200);
      (await response.Entity.Stream.ReadToEndAsync())
        .ShouldBe(new byte[] {42});
    }
    class DemoResource {}
    class DemoResourceHandler { public DemoResource Get()
    {
      return new DemoResource();
    }}

    class DemoResourceWriter : IMediaTypeWriterAsync
    {
      public object Configuration { get; set; }
      public Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
      {
        response.Stream.WriteByte(42);
        return Task.CompletedTask;
      }
    }
  }
}