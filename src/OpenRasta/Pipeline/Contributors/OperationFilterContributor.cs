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
    private readonly Func<IEnumerable<IOperationFilter>> _operationProcessors;

    public OperationFilterContributor(IDependencyResolver resolver)
    {
      _operationProcessors = resolver.ResolveAll<IOperationFilter>;
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      context.PipelineData.OperationsAsync =
        ProcessOperations(context.PipelineData.OperationsAsync).ToList();
      return !context.PipelineData.OperationsAsync.Any()
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    private IEnumerable<IOperationAsync> ProcessOperations(IEnumerable<IOperationAsync> operations)
    {
      return _operationProcessors()
        .Aggregate(
          operations = operations.ToList(),
          (ops, filter) => filter.Process(ops)).Distinct();
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

    private IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
    {
      foreach (var filter in _operationProcessors())
        yield return filter.Process;
    }
  }
}