using System;

namespace OpenRasta.Authentication.Digest
{
    [Obsolete]
    public interface IDigestAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(DigestAuthRequestParameters header);
    }
}