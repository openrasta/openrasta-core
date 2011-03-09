JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿namespace OpenRasta.Authentication.Basic
{
    public interface IBasicAuthenticator
    {
        string Realm { get; }
        AuthenticationResult Authenticate(BasicAuthRequestHeader header);
    }
}