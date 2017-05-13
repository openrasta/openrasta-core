using System.Threading.Tasks;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class YieldBeforeNextMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly string _yieldName;

    public YieldBeforeNextMiddleware(string yieldName)
    {
      _yieldName = yieldName;
    }

    public async Task Invoke(ICommunicationContext env)
    {
      var yielder = env.Yielder(_yieldName);
      var resumer = env.Resumer(_yieldName);

      yielder.SetResult(true);
      var shoulContinue = await resumer.Task;
      if (shoulContinue)
        await Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }
}