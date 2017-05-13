using System;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  class PipelineStageAsync
  {
    readonly string _yielderName;
    readonly AspNetHost _host;
    readonly EventHandlerTaskAsyncHelper _eventHandler;

    public PipelineStageAsync(string yielderName, AspNetHost host)
    {
      _yielderName = yielderName;
      _host = host;
      _eventHandler = new EventHandlerTaskAsyncHelper(Invoke);
    }

    async Task Invoke(object sender, EventArgs e)
    {
      var env = AspNetCommunicationContext.Current;
      var yielder = env.Yielder(_yielderName);
      var runTask = _host.RaiseIncomingRequestReceived(env);
      var yielded = await Yielding.DidItYield(runTask, yielder.Task);

      if (!yielded)
        return;

      var notFound = env.OperationResult as OperationResult.NotFound;
      if (notFound?.Reason != NotFoundReason.NotMapped)
        OpenRastaModuleAsync.Pipeline.HandoverToPipeline(_yielderName, runTask, env);
      else
        env.Resumer(_yielderName).SetResult(false);
    }

    public BeginEventHandler Begin => _eventHandler.BeginEventHandler;
    public EndEventHandler End => _eventHandler.EndEventHandler;
  }
}