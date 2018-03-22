using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;
using Tests.Scenarios.HandlerSelection;
using Tests.Scenarios.HandlerThrows;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class mytest
  {
    readonly InMemoryHost _inMemoryHost;

    public mytest()
    {
      _inMemoryHost = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.PipelineContributor<MyContributor>();

        ResourceSpace.Has.ResourcesOfType<string>()
          .AtUri("/")
          .HandledBy<MyHandler>().TranscodedBy<TextPlainCodec>();
      });
    }

    [Fact]
    public void test()
    {
      var result = _inMemoryHost.Get("/").Result;
      result.ReadString().ShouldBe("hello world");
    }

    class MyContributor : IPipelineContributor
    {
      public void Initialize(IPipeline pipelineRunner)
      {
        pipelineRunner.Notify(context =>
        {
          context.OperationResult = new OperationResult.OK("hello world");
          return PipelineContinuation.RenderNow;
        }).Before<KnownStages.IHandlerSelection>().And.After<KnownStages.IUriMatching>();
      }
    }

    class MyHandler
    {
      public string Get()
      {
        return "hello world";
      }
    }
  }

  public class relative_uri : location_header<RelUriRelPath>
  {
    [Fact]
    public async Task location_is_absolute()
    {
      var r = await Response;
      var rAsync = await ResponseAsync;
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/resource/relPathResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/resource/async/relPathResource");
    }
  }
}