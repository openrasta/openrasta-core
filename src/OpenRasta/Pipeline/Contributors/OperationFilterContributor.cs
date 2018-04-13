using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationFilterContributor : KnownStages.IOperationFiltering
  {
    readonly Func<IEnumerable<IOperationFilter>> _createFilters;

    public OperationFilterContributor(Func<IEnumerable<IOperationFilter>> createFilters)
    {
      _createFilters = createFilters;
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      var ops = ProcessOperations(context.PipelineData.OperationsAsync.ToList());
      context.PipelineData.OperationsAsync = ops;

      return !ops.Any()
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    List<IOperationAsync> ProcessOperations(List<IOperationAsync> operations)
    {
      var filters = _createFilters();
      return filters
        .Distinct()
        .Aggregate(
          operations,
          (ops, filter) => ops.Any() ? filter.Process(ops).ToList() : ops)
        .Distinct()
        .ToList();
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