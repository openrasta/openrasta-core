using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationInvokerContributor : KnownStages.IOperationExecution
  {
    readonly IDependencyResolver _resolver;
    IOperationExecutor _executor;

    public OperationInvokerContributor(IOperationExecutor executor)
    {
      _executor = executor;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.NotifyAsync(ExecuteOperations).After<KnownStages.IRequestDecoding>();
    }

    async Task<PipelineContinuation> ExecuteOperations(ICommunicationContext context)
    {
      try
      {
        context.OperationResult = await _executor.Execute(context.PipelineData.OperationsAsync);
      }
      catch (InterceptorException) when (context.OperationResult != null)
      {
        return PipelineContinuation.RenderNow;
      }
      return PipelineContinuation.Continue;
    }
  }
}