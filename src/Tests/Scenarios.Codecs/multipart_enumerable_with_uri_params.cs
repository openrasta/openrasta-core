using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class multipart_enumerable_with_uri_params
  {
    [GitHubIssue(32,Skip = "Fix waiting for multipart to be looked at / into")]
    public async Task should_provide_enumerable_independently()
    {
      var server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesNamed("multipart_parser")
          .AtUri("/multipart/{id}")
          .HandledBy<MultiPartHandler>();
      });
      var response = await server.Post(
        "/multipart/1",
        contentType: "multipart/form-data;boundary=bound",
        content: @"
--bound
Content-Disposition: form-data; name=""id""

1
--bound
Content-Disposition: form-data; name=""another""

another
--bound--
");
      response.StatusCode.ShouldBe(200);
      response.ReadString().ShouldBe("id=1;count=1");
    }

    class MultiPartHandler
    {
      public string Post(int id, IEnumerable<IMultipartHttpEntity> entities)
      {
        return $"id={id};count={entities.Count()}";
      }
    }
  }
}