using System.Web;
using OpenRasta.Diagnostics;
using OpenRasta.DI;

namespace OpenRasta.Hosting.AspNet
{
    public class OpenRastaRewriterHandler : IHttpHandler
    {
        public OpenRastaRewriterHandler()
        {
            Log = NullLogger.Instance;
        }

        public bool IsReusable => true;

        public ILogger Log { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            using (Log.Operation(this, "Rewriting to original path"))
            {
                HttpContext.Current.RewritePath((string) HttpContext.Current.Items[OpenRastaModule.ORIGINAL_PATH_KEY],
                    false);
                OpenRastaModule.HostManager.Resolver.Resolve<OpenRastaIntegratedHandler>().ProcessRequest(context);
            }
        }
    }
}