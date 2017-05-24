using System.Threading.Tasks;
using System.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaHandlerAsync : HttpTaskAsyncHandler
  {
    readonly Task _pipeline;
    readonly TaskCompletionSource<bool> _resumer;

    public OpenRastaHandlerAsync(Task pipeline, TaskCompletionSource<bool> resumerCompletion)
    {
      _pipeline = pipeline;
      _resumer = resumerCompletion;
    }
    
    public override async Task ProcessRequestAsync(HttpContext context)
    {
      _resumer.TrySetResult(true);
      await _pipeline;
    }
  }
}