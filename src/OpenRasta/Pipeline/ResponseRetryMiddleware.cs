using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class ResponseRetryMiddleware : IPipelineMiddlewareFactory, IPipelineMiddleware
  {
    IPipelineMiddleware _responsePipeline;

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      _responsePipeline = next;
      return this;
    }

    public Task Invoke(ICommunicationContext env)
    {
      return _responsePipeline.Invoke(env);
    }
  }
}