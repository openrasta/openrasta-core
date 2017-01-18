using System.Web;
using OpenRasta.DI;

namespace OpenRasta.Hosting.AspNet
{
    // ReSharper disable once UnusedMember.Global
    public class OpenRastaHandler : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            if (context.Items[OpenRastaModule.ORIGINAL_PATH_KEY] != null)
                return OpenRastaModule.HostManager.Resolver.Resolve<OpenRastaRewriterHandler>();
            return OpenRastaModule.HostManager.Resolver.Resolve<OpenRastaIntegratedHandler>();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
        }
