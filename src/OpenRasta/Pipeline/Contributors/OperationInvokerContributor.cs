using System;
using System.Threading.Tasks;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationInvokerContributor : KnownStages.IOperationExecution
  {
    readonly Func<IOperationExecutor> _executorFactory;

    public OperationInvokerContributor(Func<IOperationExecutor> executorFactory)
    {
      _executorFactory = executorFactory;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.NotifyAsync(ExecuteOperations);
    }

    async Task<PipelineContinuation> ExecuteOperations(ICommunicationContext context)
    {
      var executor = _executorFactory();
      try
      {
        context.OperationResult = await executor.Execute(context.PipelineData.OperationsAsync);
      }
      catch (InterceptorException) when (context.OperationResult != null)
      {
        return PipelineContinuation.RenderNow;
      }
      return PipelineContinuation.Continue;
    }
  }
}
