using System;

namespace OpenRasta.Authentication.Digest
{
    [Obsolete("Authentication features are moving to a new package, see more information at http://https://github.com/openrasta/openrasta/wiki/Authentication")]
    public interface IDigestAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(DigestAuthRequestParameters header);
    }
}