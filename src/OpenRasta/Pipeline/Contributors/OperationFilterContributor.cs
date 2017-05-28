using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationFilterContributor : KnownStages.IOperationFiltering
  {
    readonly Func<IEnumerable<IOperationFilter>> _filters;

    public OperationFilterContributor(Func<IEnumerable<IOperationFilter>> filters)
    {
      _filters = filters;
    }


    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      context.PipelineData.OperationsAsync =
        ProcessOperations(context.PipelineData.OperationsAsync)
          .ToList();
      
      return !context.PipelineData.OperationsAsync.Any() 
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    IEnumerable<IOperationAsync> ProcessOperations(IEnumerable<IOperationAsync> operations)
    {
      var chain = GetMethods().Chain();
      return chain == null ? new IOperationAsync[0] : chain(operations);
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationCreation>();;
    }

    protected virtual PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.MethodNotAllowed();
      return PipelineContinuation.RenderNow;
    }

    IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
    {
      foreach (var filter in _filters())
        yield return filter.Process;
    }
  }
}