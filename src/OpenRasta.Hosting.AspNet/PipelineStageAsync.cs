using System;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  class PipelineStageAsync
  {
    readonly string _yielderName;
    readonly IPipelineMiddleware _middleware;
    readonly EventHandlerTaskAsyncHelper _eventHandler;

    public PipelineStageAsync(string yielderName, IPipelineMiddleware middleware)
    {
      _yielderName = yielderName;
      _middleware = middleware;
      _eventHandler = new EventHandlerTaskAsyncHelper(Invoke);
    }

    async Task Invoke(object sender, EventArgs e)
    {
      var env = OpenRastaModule.CommunicationContext;
      var yielder = env.Yielder(_yielderName);
      var yielded = await Yielding.DidItYield(_middleware.Invoke(env), yielder.Task);

      if (!yielded)
        return;

      var notFound = env.OperationResult as OperationResult.NotFound;
      if (notFound?.Reason != NotFoundReason.NotMapped)
        OpenRastaModuleAsync.Pipeline.HandoverToPipeline();
    }

    public BeginEventHandler Begin => _eventHandler.BeginEventHandler;
    public EndEventHandler End => _eventHandler.EndEventHandler;
  }
}