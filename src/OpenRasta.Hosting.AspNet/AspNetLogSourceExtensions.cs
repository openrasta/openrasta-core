using OpenRasta.Diagnostics;

namespace OpenRasta.Hosting.AspNet
{
    public static class AspNetLogSourceExtensions
    {
        public static void IgnoredRequest(this ILogger<AspNetLogSource> log)
        {
            log.WriteDebug("Request ignored.");
        }

        public static void IisDetected(this ILogger<AspNetLogSource> log, AspNetPipeline iisVersion, string productHeader)
        {
            log.WriteDebug("Loaded IIS abstraction {0} for product header \"{1}\".", iisVersion.GetType().Name, productHeader);
        }

        public static void PathRewrote(this ILogger<AspNetLogSource> log)
        {
            log.WriteDebug("Rewrote path.");
        }

        public static void StartPreExecution(this ILogger<AspNetLogSource> log)
        {
            log.WriteDebug("Starts pre-executing the request.");
        }
    }
}