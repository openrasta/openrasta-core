using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CleanupMiddleware : AbstractMiddleware
  {
    public override async Task Invoke(ICommunicationContext env)
    {
      try
      {
        await Next.Invoke(env);
      }
      finally
      {
        env.Request.Entity?.Dispose();
        env.Response.Entity?.Dispose();
      }
    }
  }
}