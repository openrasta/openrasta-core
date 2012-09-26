using System.Security.Principal;

namespace OpenRasta.Authentication
{
    public interface IPrincipalProvider
    {
        IPrincipal Get(AuthenticationResult.Success authResult, IAuthenticationScheme scheme);
    }
}