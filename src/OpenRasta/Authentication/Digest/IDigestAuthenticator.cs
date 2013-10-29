using System;

namespace OpenRasta.Authentication.Digest
{
   [Obsolete("This class is obsolete. See https://github.com/openrasta/openrasta/wiki/FAQs", false)] 
    public interface IDigestAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(DigestAuthRequestParameters header);
    }
}