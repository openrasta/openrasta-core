using System.Threading.Tasks;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace Tests.Pipeline.Middleware.Interception
{
  public class SimpleMiddleware : IPipelineMiddleware
  {
    readonly IPipelineMiddleware _next;

    public SimpleMiddleware(IPipelineMiddleware next)
    {
      _next = next;
    }

    public Task Invoke(ICommunicationContext env)
    {
      return _next.Invoke(env);
    }
  }
}