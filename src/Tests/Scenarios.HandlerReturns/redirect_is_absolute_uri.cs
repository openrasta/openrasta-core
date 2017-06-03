using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.HandlerReturns
{
  public class redirect_is_absolute_uri
  {
    [Fact]
    public async Task location_header_abs_path_is_relative_to_app_base()
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .HandledBy<RelUriAbsPath>()) { ApplicationVirtualPath = "/myApp/" };
      (await server.Get("/resource/")).Headers["Location"].ShouldBe("http://localhost/myApp/absPathResource");
    }
    [Fact]
    public async Task location_header_is_absolute_uri()
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .HandledBy<AbsUriHandler>());
      (await server.Get("/resource/")).Headers["Location"].ShouldBe("http://localhost/absResource");
    }
    [Fact]
    public async Task location_header_is_relative_uri_abs_path()
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .HandledBy<RelUriAbsPath>());
      (await server.Get("/resource/")).Headers["Location"].ShouldBe("http://localhost/absPathResource");
    }
    [Fact]
    public async Task location_header_is_relative_uri_rel_path()
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .HandledBy<RelUriRelPath>());
      (await server.Get("/resource/")).Headers["Location"].ShouldBe("http://localhost/resource/relPathResource");
    }

    class RelUriAbsPath
    {
      public OperationResult Get()
      {
        return new OperationResult.OK { RedirectLocation = new Uri("/absPathResource",  UriKind.Relative) };
      }
      
    }

    class RelUriRelPath
    {
      public OperationResult Get()
      {
        return new OperationResult.OK { RedirectLocation = new Uri("relPathResource",  UriKind.Relative) };
      }
      
    }
    class AbsUriHandler
    {
      public OperationResult Get()
      {
        return new OperationResult.OK { RedirectLocation = new Uri("http://localhost/absResource",  UriKind.Absolute) };
      }
    }
  }
}