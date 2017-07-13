using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationFilterContributor : KnownStages.IOperationFiltering
  {
    private readonly Func<IEnumerable<IOperationFilter>> _createFilters;

    public OperationFilterContributor(IDependencyResolver resolver)
    {
      _createFilters = ()=>resolver.ResolveAll<IOperationFilter>();
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      var ops = ProcessOperations(context.PipelineData.OperationsAsync.ToList());
      context.PipelineData.OperationsAsync = ops;
      
      return !ops.Any()
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    private List<IOperationAsync> ProcessOperations(List<IOperationAsync> operations)
    {
      return _createFilters()
       .Aggregate(
          operations,
          (ops, filter) => filter.Process(ops).ToList())
        .Distinct()
        .ToList();
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationCreation>();
    }

    protected virtual PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.MethodNotAllowed();
      return PipelineContinuation.RenderNow;
    }
  }
}