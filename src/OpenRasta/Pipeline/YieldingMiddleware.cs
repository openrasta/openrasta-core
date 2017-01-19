using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class YieldingMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly IPipelineMiddleware _inner;

    public YieldingMiddleware(IPipelineMiddleware inner)
    {
      _inner = inner;
    }
    public async Task Invoke(ICommunicationContext env)
    {
      await _inner.Invoke(env);
      var suspend = env.PipelineData.Suspend;
      var resume = env.PipelineData.Resume;
      if (suspend != null && resume != null)
      {
        suspend.SetResult(null);
        await resume.Task;
      }
      await Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      this.Next = next;
      return this;
    }

    public IPipelineMiddleware Next { get; set; }
  }
}