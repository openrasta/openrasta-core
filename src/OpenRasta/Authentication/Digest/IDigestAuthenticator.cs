using System;

namespace OpenRasta.Authentication.Digest
{
    [Obsolete(ObsoletedContent.Authentication)]
    public interface IDigestAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(DigestAuthRequestParameters header);
    }
}