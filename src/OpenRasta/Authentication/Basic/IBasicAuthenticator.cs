using System;

namespace OpenRasta.Authentication.Basic
{
    [Obsolete]
    public interface IBasicAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(BasicAuthRequestHeader header);
    }
}