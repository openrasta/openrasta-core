using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class codec_used_with_optional_parameters
  {
    InMemoryHost _server;

    public codec_used_with_optional_parameters()
    {
      _server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesOfType<Kuku>()
          .AtUri("/old/{id}?a={a}")
          .And.AtUri("/new/{idWithNewOptional}?a={a}")
          .HandledBy<KukuHandler>()
          .AsJsonDataContract();
      });
    }

    [GitHubIssue(42)]
    public async Task parses_to_the_correct_parameter_for_old_style()
    {
      var response = await _server.Put("/old/1", "{ }", contentType: "application/json");
      response.StatusCode.ShouldBe(200);
      response.ReadString().ShouldBe("old:id=1;a=False;dto=dto");
    }
    
    [GitHubIssue(42)]
    public async Task parses_to_the_correct_parameter_for_new_style()
    {
      var response = await _server.Put("/new/1", "{ }", contentType: "application/json");
      response.StatusCode.ShouldBe(200);
      response.ReadString().ShouldBe("new:id=1;a=False;dto=dto");
    }
    
    public class Kuku
    {
    }

    public class KukuHandler
    {
      public string Put(int idWithNewOptional, Kuku dto, bool a = false)
      {
        return $"new:id={idWithNewOptional};a={a};dto={(dto == null ? "null" : "dto")}";
      }
      public string Put(int id, [Optional, DefaultParameterValue(false)] bool a, Kuku dto)
      {
        return $"old:id={id};a={a};dto={(dto == null ? "null" : "dto")}";
      }
    }
  }
}