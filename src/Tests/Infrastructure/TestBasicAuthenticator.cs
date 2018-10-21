using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;

namespace Tests.Infrastructure
{
  public class TestBasicAuthenticator : IBasicAuthenticator
  {
    public string Realm => "TestRealm";

    public AuthenticationResult Authenticate(BasicAuthRequestHeader header) => new AuthenticationResult.Success(header.Username);
  }
}