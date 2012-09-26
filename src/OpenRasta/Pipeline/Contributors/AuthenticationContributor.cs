using System;
using System.Linq;
using OpenRasta.Authentication;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
    public class AuthenticationContributor : KnownStages.IAuthentication
    {
        readonly IDependencyResolver _resolver;
        readonly IPrincipalProvider _principalProvider;

        public ILogger Log { get; set; }

        public AuthenticationContributor(IDependencyResolver resolver, IPrincipalProvider principalProvider)
        {
            _resolver = resolver;
            _principalProvider = principalProvider;
        }

        public void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(AuthoriseRequest)
                .After<KnownStages.IBegin>()
                .And
                .Before<KnownStages.IHandlerSelection>();
        }

        private PipelineContinuation AuthoriseRequest(ICommunicationContext context)
        {
            var requestedAuthSchemeName= ExtractRequestedAuthScheme(context.Request);

            if (requestedAuthSchemeName == null)
                return PipelineContinuation.Continue;

            var authenticators = _resolver.ResolveAll<IAuthenticationScheme>();

            var schemeToUse = authenticators.SingleOrDefault(scheme => string.Equals(scheme.Name, requestedAuthSchemeName, StringComparison.InvariantCultureIgnoreCase));

            if(schemeToUse == null)
            {
                context.Response.Headers["Warning"] = "Unsupported Authentication Scheme";
                return PipelineContinuation.Continue;
            }

            var authResult = schemeToUse.Authenticate(context.Request);

            if (authResult is AuthenticationResult.Success)
            {
                var success = (authResult as AuthenticationResult.Success);
                context.User = _principalProvider.Get(success, schemeToUse);
            }

            if (authResult is AuthenticationResult.MalformedCredentials)
            {
                context.OperationResult = new OperationResult.BadRequest();
                context.Response.Headers["Warning"] = "Malformed credentials";
                return PipelineContinuation.RenderNow;
            }

            if (authResult is AuthenticationResult.Failed)
            {
                context.OperationResult = new OperationResult.Unauthorized();
            }

            return PipelineContinuation.Continue;
        }

        static string ExtractRequestedAuthScheme(IRequest request)
        {
            var authRequestHeader = request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authRequestHeader))
                return null;

            var requestedAuthSchemeName = authRequestHeader.Split(' ')[0];

            if (string.IsNullOrEmpty(requestedAuthSchemeName))
                return null;

            return requestedAuthSchemeName;
        }
    }
}