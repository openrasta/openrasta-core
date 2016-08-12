using System;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Diagnostics;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaIntegratedHandler : HttpTaskAsyncHandler
  {
    public OpenRastaIntegratedHandler()
    {
      Log = NullLogger.Instance;
    }

    public override bool IsReusable { get; } = true;
    public ILogger Log { get; set; }

    public override Task ProcessRequestAsync(HttpContext context)
    {
      using (Log.Operation(this, "Request for {0}".With(context.Request.Url)))
      {
        var env = OpenRastaModule.CommunicationContext;
        OpenRastaModule.Host.RaiseIncomingRequestReceived(env);
        var task = env.PipelineData["openrasta.pipeline.completion"] as Task;
        return task ?? Task.FromResult(0);
      }
    }
  }
}
