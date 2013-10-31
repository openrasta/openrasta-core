using System;

namespace OpenRasta.Authentication.Basic
{
    [Obsolete("This class is obsolete. See https://github.com/openrasta/openrasta/wiki/FAQs", false)] 
    public interface IBasicAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(BasicAuthRequestHeader header);
    }
}