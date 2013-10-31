using System;

namespace OpenRasta.Authentication.Basic
{
    [Obsolete(ObsoletedContent.Authentication)]
    public interface IBasicAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(BasicAuthRequestHeader header);
    }
}