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

    public OpenRastaHandlerAsync(Task pipeline)
    {
      _pipeline = pipeline;
      Log = NullLogger.Instance;
    }

    public OpenRastaHandlerAsync(IntegratedPipeline pipeline, string yielderName, Task runTask, ICommunicationContext env)
    {
      _yielderName = yielderName;
      _env = env;
    }

    public ILogger Log { get; set; }

    public override Task ProcessRequestAsync(HttpContext context)
    {
      var resumer = _env.Resumer(_yielderName);
      resumer.SetResult(true);
      return _pipeline;
    }
  }
}