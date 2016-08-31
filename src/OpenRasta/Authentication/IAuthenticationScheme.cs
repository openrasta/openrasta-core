using System;
using OpenRasta.Web;

namespace OpenRasta.Authentication
{
    [Obsolete("Authentication features are moving to a new package, see more information at http://https://github.com/openrasta/openrasta/wiki/Authentication")]
    public interface IAuthenticationScheme
    {
        string Name { get; }

        AuthenticationResult Authenticate(IRequest request);

        void Challenge(IResponse response);
    }
}