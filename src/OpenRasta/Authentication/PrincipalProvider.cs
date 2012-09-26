using System.Security.Principal;

namespace OpenRasta.Authentication
{
    public class PrincipalProvider : IPrincipalProvider
    {
        public IPrincipal Get(AuthenticationResult.Success authResult, IAuthenticationScheme scheme)
        {
            var identity = new GenericIdentity(authResult.Username, scheme.Name);
            return new GenericPrincipal(identity, authResult.Roles);
        }
    }
}