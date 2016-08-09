using System;

namespace OpenRasta.Authentication.Basic
{
    [Obsolete("Authentication features are moving to a new package, see more information at http://https://github.com/openrasta/openrasta/wiki/Authentication")]
    public interface IBasicAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(BasicAuthRequestHeader header);
    }
}