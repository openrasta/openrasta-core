using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class multipart_invalid_content
  {
    [Fact]
    public async Task should_return_400()
    {
      var server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesNamed("multipart_parser")
          .AtUri("/multipart/")
          .HandledBy<DoNothing>();
      });
      var response = await server.Post(
        "/multipart/",
        contentType: "multipart/form-data;boundary=bound4",
        content: @"
--bound4
Content-Disposition: form-data; name=""id""

1
");
      response.StatusCode.ShouldBe(400);
    }

    class DoNothing
    {
      public void Post(string id){ }
    }
  }
}