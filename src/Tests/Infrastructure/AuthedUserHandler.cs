using OpenRasta.Web;

namespace Tests.Infrastructure
{
  public class AuthedUserHandler
  {
    readonly ICommunicationContext context;

    public AuthedUserHandler(ICommunicationContext context)
    {
      this.context = context;
    }

    public string Get()
    {
      return this.context.User.Identity.Name;
    }
  }
}