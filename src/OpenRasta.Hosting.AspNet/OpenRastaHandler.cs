using System.Web;
using OpenRasta.Diagnostics;

namespace OpenRasta.Hosting.AspNet
{
    public class OpenRastaHandler : IHttpHandler
    {
        readonly AspNetPipeline _pipeline;

        public OpenRastaHandler(AspNetPipeline pipeline)
        {
            _pipeline = pipeline;
            Log = NullLogger.Instance;
        }

        public bool IsReusable => true;

        public ILogger Log { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            _pipeline.HandoverFromPipeline();
            using (Log.Operation(this, "Request for {0}".With(context.Request.Url)))
        {
                OpenRastaModule.Host.RaiseIncomingRequestReceived(OpenRastaModule.CommunicationContext);
            }
        }
    }
}