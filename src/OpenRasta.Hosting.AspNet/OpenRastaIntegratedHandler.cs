using System.Web;
using OpenRasta.Diagnostics;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaIntegratedHandler : IHttpHandler
  {
    public OpenRastaIntegratedHandler()
    {
      Log = NullLogger.Instance;
    }
    public bool IsReusable
    {
      get { return true; }
    }

    public ILogger Log { get; set; }

    public void ProcessRequest(HttpContext context)
    {
      using (Log.Operation(this, "Request for {0}".With(context.Request.Url)))
      {
        OpenRastaModule.Host.RaiseIncomingRequestReceived(OpenRastaModule.CommunicationContext);
      }
    }
  }
}