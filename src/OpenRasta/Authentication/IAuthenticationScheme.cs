using System;
using OpenRasta.Web;

namespace OpenRasta.Authentication
{
    [Obsolete]
    public interface IAuthenticationScheme
    {
        string Name { get; }

        AuthenticationResult Authenticate(IRequest request);

        void Challenge(IResponse response);
    }
}