using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.Pipeline.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationCreatorContributor : KnownStages.IOperationCreation
  {
    readonly IOperationCreator _creator;

    public OperationCreatorContributor(IOperationCreator creator)
    {
      _creator = creator;
      Logger = NullLogger<PipelineLogSource>.Instance;
    }

    ILogger<PipelineLogSource> Logger { get; }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(CreateOperations);
    }

    PipelineContinuation CreateOperations(ICommunicationContext context)
    {
      var uriModel = context.PipelineData.SelectedResource?.UriModel?.Operations;
      if (uriModel != null)
      {
        context.PipelineData.OperationsAsync = _creator.CreateOperations(uriModel).ToList();
      }
      // for compat because tests need refactoring
      else if (context.PipelineData.SelectedHandlers == null)
      {
        return PipelineContinuation.Continue;
      }
      else if (context.PipelineData.SelectedHandlers != null)
      {
        context.PipelineData.OperationsAsync =
          _creator.CreateOperations(context.PipelineData.SelectedHandlers).ToList();
      }

      LogOperations(context.PipelineData.OperationsAsync);
      if (context.PipelineData.OperationsAsync.Any())
        return PipelineContinuation.Continue;
      context.OperationResult = CreateMethodNotAllowed(context);
      return PipelineContinuation.RenderNow;
    }

    static OperationResult.MethodNotAllowed CreateMethodNotAllowed(ICommunicationContext context)
    {
      return new OperationResult.MethodNotAllowed(context.Request.Uri, context.Request.HttpMethod,
        context.PipelineData.ResourceKey);
    }

    void LogOperations(IEnumerable<IOperationAsync> operations)
    {
      foreach (var operation in operations)
        Logger.WriteDebug("Created operation named {0} with signature {1}", operation.Name, operation.ToString());
    }
  }
}