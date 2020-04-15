using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationFilterContributor : KnownStages.IOperationFiltering
  {
    readonly Func<IOperationFilter> _createFilters;

    public OperationFilterContributor(Func<IOperationFilter> createFilters)
    {
      _createFilters = createFilters;
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      var filters = _createFilters();
      
      var ops = filters.Process(context.PipelineData.OperationsAsync).ToList();
      
      context.PipelineData.OperationsAsync = ops;

      return !ops.Any()
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations);
    }

    protected virtual PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.MethodNotAllowed();
      return PipelineContinuation.RenderNow;
    }
  }
}