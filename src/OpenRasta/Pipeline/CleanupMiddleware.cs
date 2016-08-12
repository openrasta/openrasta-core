using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CleanupMiddleware : IPipelineMiddleware
  {
    public Task Invoke(ICommunicationContext env)
    {
      env.Request.Entity?.Dispose();
      env.Response.Entity?.Dispose();
      return Task.FromResult(0);
    }
  }
}