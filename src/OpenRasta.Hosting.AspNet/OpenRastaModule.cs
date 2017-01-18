using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OpenRastaModule : IHttpModule
    {
        const string COMM_CONTEXT_KEY = "__OR_COMM_CONTEXT";
        internal const string ORIGINAL_PATH_KEY = "__ORIGINAL_PATH";

        public static HostManager HostManager => HostManagerImplementation.Value;

        static readonly Lazy<HostManager> HostManagerImplementation =
            new Lazy<HostManager>(CreateHost, LazyThreadSafetyMode.PublicationOnly);

        bool _disposed;

        static OpenRastaModule()
        {
            Host = new AspNetHost();
            Log = NullLogger<AspNetLogSource>.Instance;
        }

        public static AspNetCommunicationContext CommunicationContext
        {
            get
            {
                var context = HttpContext.Current;
                if (context.Items.Contains(COMM_CONTEXT_KEY))
                    return (AspNetCommunicationContext) context.Items[COMM_CONTEXT_KEY];
                var orContext = new AspNetCommunicationContext(Log,
                    context,
                    new AspNetRequest(context),
                    new AspNetResponse(context) {Log = Log});
                context.Items[COMM_CONTEXT_KEY] = orContext;

                return orContext;
            }
        }

        public static AspNetHost Host { get; }

        static ILogger<AspNetLogSource> Log { get; set; }

        public void Dispose()
        {
            // Note we do not unsubscribe from events on HttpApplication as that instance is going down and it'd cause an exception.
            if (_disposed) return;

            _disposed = true;
            Host.RaiseStop();
        }

        public void Init(HttpApplication app)
        {
            app.PostResolveRequestCache += HandleHttpApplicationPostResolveRequestCacheEvent;
            app.EndRequest += HandleHttpApplicationEndRequestEvent;
        }

        static readonly Lazy<AspNetPipeline> _pipeline = new Lazy<AspNetPipeline>(() =>
            HttpRuntime.UsingIntegratedPipeline
                ? (AspNetPipeline) new IntegratedPipeline()
                : new ClassicPipeline(), LazyThreadSafetyMode.PublicationOnly);

        static AspNetPipeline Pipeline => _pipeline.Value;

        static HostManager CreateHost()
        {
            var hostManager = HostManager.RegisterHost(Host);
            try
            {
                Host.RaiseStart();
                Log = hostManager.Resolver.Resolve<ILogger<AspNetLogSource>>() ?? Log;
            }
            catch
            {
                HostManager.UnregisterHost(Host);
                throw;
            }
            return hostManager;
        }

        static bool HandlerAlreadyMapped(string method, Uri path)
        {
            return Pipeline.IsHandlerAlreadyRegisteredForRequest(method, path);
        }


        static void HandleHttpApplicationEndRequestEvent(object sender, EventArgs e)
        {

            if (!HttpContext.Current.Items.Contains(ORIGINAL_PATH_KEY)) return;
            var commContext = (ICommunicationContext) ((HttpApplication) sender).Context.Items[COMM_CONTEXT_KEY];
            Host.RaiseIncomingRequestProcessed(commContext);
        }

        void HandleHttpApplicationPmEvent(object sender, EventArgs e)
        {
            if (ShouldIgnoreRequestEarly())
            {
                Log.IgnoredRequest();
                return;
            }

            //else continue processing with OpenRasta

            Log.StartPreExecution();
            var context = CommunicationContext;
            var stage = context.PipelineData.PipelineStage;
            if (stage == null)
                context.PipelineData.PipelineStage = stage =
                    new PipelineStage(HostManager.Resolver.Resolve<IPipeline>());
            stage.SuspendAfter<KnownStages.IUriMatching>();
            Host.RaiseIncomingRequestReceived(context);

            if (!ResourceFound(context) && !OperationResultSetByCode(context))
            {
                Log.IgnoredRequest();
                return;
            }

            HttpContext.Current.Items[ORIGINAL_PATH_KEY] = HttpContext.Current.Request.Path;

            HttpContext.Current.RewritePath(VirtualPathUtility.ToAppRelative("~/openrasta.axd"), false);
            Log.PathRewrote();
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

        static bool OperationResultSetByCode(AspNetCommunicationContext context)
        {
            return context.OperationResult != null
                   && (context.OperationResult as OperationResult.NotFound)?.Reason != NotFoundReason.NotMapped;
        }

        static bool ResourceFound(AspNetCommunicationContext context)
        {
            return context.PipelineData.ResourceKey != null;
        }

        static bool OverrideHttpHandlers => WebConfigurationManager.AppSettings[
                                                "openrasta.hosting.aspnet.paths.handlers"] == "all";

        static bool HandleDirectories => WebConfigurationManager.AppSettings[
                                             "openrasta.hosting.aspnet.paths.directories"] == "all";

        static bool HandleRootPath => WebConfigurationManager.AppSettings[
                                          "openrasta.hosting.aspnet.paths.root"] != "disable";
    }
}