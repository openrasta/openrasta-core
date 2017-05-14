using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using OpenRasta.DI;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  class PipelineStageAsync
  {
    readonly string _yielderName;
    readonly AspNetHost _host;
    readonly Lazy<AspNetPipeline> _pipeline;
    readonly EventHandlerTaskAsyncHelper _eventHandler;

    public PipelineStageAsync(string yielderName, AspNetHost host, Lazy<AspNetPipeline> pipeline)
    {
      _yielderName = yielderName;
      _host = host;
      _pipeline = pipeline;
      _eventHandler = new EventHandlerTaskAsyncHelper(Invoke);
    }

    async Task Invoke(object sender, EventArgs e)
    {
      if (ShouldIgnoreRequestEarly()) return;

      var env = AspNetCommunicationContext.Current;
      var yielder = env.Yielder(_yielderName);
      try
      {
        var runTask = _host.RaiseIncomingRequestReceived(env);
        var yielded = await Yielding.DidItYield(runTask, yielder.Task);

        if (!yielded)
          return;
        var notFound = env.OperationResult as OperationResult.NotFound;
        if (notFound?.Reason != NotFoundReason.NotMapped)
          OpenRastaModuleAsync.Pipeline.HandoverToPipeline(_yielderName, runTask, env);
        else
          env.Resumer(_yielderName).SetResult(false);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public BeginEventHandler Begin => _eventHandler.BeginEventHandler;
    public EndEventHandler End => _eventHandler.EndEventHandler;

    bool HandlerAlreadyMapped(string method, Uri path)
    {
      return _pipeline.Value.IsHandlerAlreadyRegisteredForRequest(method, path);
    }

    bool ShouldIgnoreRequestEarly()
    {
      if (RequestIsForExistingFile(HttpContext.Current.Request.Path))
        return true;

      if (HandleRootPath == false && RequestIsRootPath(HttpContext.Current.Request.Path))
        return true;

      if (HandleDirectories == false && RequestIsRootPath(HttpContext.Current.Request.Path) == false &&
          RequestIsForExistingDirectory(HttpContext.Current.Request.Path))
        return true;

      if (OverrideHttpHandlers == false &&
          HandlerAlreadyMapped(HttpContext.Current.Request.HttpMethod, HttpContext.Current.Request.Url))
        return true;

      return false;
    }

    static bool RequestIsForExistingFile(string requestPath)
    {
      return HostingEnvironment.VirtualPathProvider.FileExists(requestPath);
    }

    static bool RequestIsForExistingDirectory(string requestPath)
    {
      return HostingEnvironment.VirtualPathProvider.DirectoryExists(requestPath);
    }

    static bool RequestIsRootPath(string requestPath)
    {
      return requestPath == "/";
    }

    static bool OverrideHttpHandlers => WebConfigurationManager.AppSettings[
                                          "openrasta.hosting.aspnet.paths.handlers"] == "all";

    static bool HandleDirectories => WebConfigurationManager.AppSettings[
                                       "openrasta.hosting.aspnet.paths.directories"] == "all";

    static bool HandleRootPath => WebConfigurationManager.AppSettings[
                                    "openrasta.hosting.aspnet.paths.root"] != "disable";
  }
}