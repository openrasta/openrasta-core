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
    private IDependencyResolver _resolver;

    public OperationFilterContributor(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    protected void InitializeWhen(IPipelineExecutionOrder pipeline)
    {
      pipeline.After<KnownStages.IOperationCreation>();
    }

    public virtual PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      context.PipelineData.OperationsAsync =
        ProcessOperations(context.PipelineData.OperationsAsync).ToList<IOperationAsync>();
      if (!context.PipelineData.OperationsAsync.Any())
        return OnOperationsEmpty(context);
      return OnOperationProcessingComplete(context.PipelineData.OperationsAsync) ?? PipelineContinuation.Continue;
    }

    public virtual IEnumerable<IOperationAsync> ProcessOperations(IEnumerable<IOperationAsync> operations)
    {
      var chain = GetMethods().Chain();
      return chain == null ? new IOperationAsync[0] : chain(operations);
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      InitializeWhen(pipelineRunner.Notify(ProcessOperations));
    }

    protected virtual PipelineContinuation? OnOperationProcessingComplete(IEnumerable<IOperationAsync> ops)
    {
      return null;
    }

    protected virtual PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.MethodNotAllowed();

      return PipelineContinuation.RenderNow;
    }

    private IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
    {
      var operationProcessors = _resolver.ResolveAll<IOperationFilter>();

      foreach (var filter in operationProcessors)
        yield return filter.Process;
    }
  }
}