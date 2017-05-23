using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaHandlerAsync : HttpTaskAsyncHandler
  {
    readonly string _yielderName;
    readonly ICommunicationContext _env;
    readonly Task _pipeline;

    public OpenRastaHandlerAsync(string yielderName, ICommunicationContext env, Task pipeline)
    {
      _yielderName = yielderName;
      _env = env;

      _pipeline = pipeline;
      Log = NullLogger.Instance;
    }

    public ILogger Log { get; set; }
    
    public override async Task ProcessRequestAsync(HttpContext context)
    {
      _env.PipelineData["openrasta.hosting.aspnet.handled"] = true;
      
      var resumer = _env.Resumer(_yielderName);
      var mid = Thread.CurrentThread.ManagedThreadId;

      resumer.TrySetResult(true);
      await _pipeline;
    }
  }
}