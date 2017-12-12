using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Tests.Scenarios.HandlerSelection;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public abstract class location_header<T> : IDisposable
  {
    protected readonly Task<IResponse> Response;
    protected readonly Task<IResponse> ResponseAsync;
    private InMemoryHost _server;

    protected location_header(string appPath = null)
    {
      _server = new InMemoryHost(() =>
        ResourceSpace.Has.ResourcesNamed("root")
          .AtUri("/resource/")
          .And.AtUri("/resource/async/").Named("async")
          .HandledBy<T>()) {ApplicationVirtualPath = appPath ?? "/"};

      Response = _server.Get($"{_server.ApplicationVirtualPath}resource/");
      ResponseAsync = _server.Get($"{_server.ApplicationVirtualPath}resource/async/");
    }

    public void Dispose()
    {
      _server.Close();
    }
  }
}