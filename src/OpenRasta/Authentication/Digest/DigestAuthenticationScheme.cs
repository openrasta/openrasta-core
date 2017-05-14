using System;
using OpenRasta.Authentication.Basic;
using OpenRasta.Web;

namespace OpenRasta.Authentication.Digest
{
    [Obsolete("Authentication features are moving to a new package, see more information at http://https://github.com/openrasta/openrasta/wiki/Authentication")]
    public class DigestAuthenticationScheme : IAuthenticationScheme
    {
        private readonly IDigestAuthenticator _digestAuthenticator;

        public string Name { get { return "Basic"; } }

        public DigestAuthenticationScheme(IDigestAuthenticator digestAuthenticator)
        {
            _digestAuthenticator = digestAuthenticator;
        }

        public AuthenticationResult Authenticate(IRequest request)
        {
            DigestAuthRequestParameters credentials;

            if (DigestAuthRequestParameters.TryParse(request.Headers["Authorization"], out credentials))
            {
                return _digestAuthenticator.Authenticate(credentials);
            }

            return new AuthenticationResult.MalformedCredentials();
        }

        public void Challenge(IResponse response)
        {
            response.Headers["WWW-Authenticate"] = $"Basic realm=\"{_digestAuthenticator.Realm}\"";
        }
    }
}