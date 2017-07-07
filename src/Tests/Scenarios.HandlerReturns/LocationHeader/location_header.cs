using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Tests.Scenarios.HandlerSelection;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public abstract class location_header<T>
  {
    protected readonly Task<IResponse> Response;
    protected readonly Task<IResponse> ResponseAsync;

    protected location_header(string appPath = null)
    {
      var server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .And.AtUri("/resource/async/").Named("async")
          .HandledBy<T>()) {ApplicationVirtualPath = appPath ?? "/"};

      Response = server.Get($"{server.ApplicationVirtualPath}resource/");
      ResponseAsync = server.Get($"{server.ApplicationVirtualPath}resource/async/");
      server.Close();
    }
  }
}