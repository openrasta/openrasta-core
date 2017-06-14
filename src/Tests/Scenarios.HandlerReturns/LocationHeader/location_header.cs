using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Tests.Scenarios.HandlerSelection;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public abstract class location_header<T>
  {
    protected readonly IResponse Response;
    protected readonly IResponse ResponseAsync;

    protected location_header(string appPath = null)
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .And.AtUri("/resource/async/").Named("async")
          .HandledBy<T>()) {ApplicationVirtualPath = appPath ?? "/"};

      Response = server.Get($"{server.ApplicationVirtualPath}resource/").Result;
      ResponseAsync = server.Get($"{server.ApplicationVirtualPath}resource/async/").Result;
    }
  }
}