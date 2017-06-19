using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestCodecSelector
    : KnownStages.ICodecRequestSelection
  {
    private readonly IDependencyResolver _resolver;

    public RequestCodecSelector(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    static PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.RequestMediaTypeUnsupported();
      return PipelineContinuation.RenderNow;
    }

    private PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      context.PipelineData.OperationsAsync = ProcessOperations(context.PipelineData.OperationsAsync).ToList();
      if (!context.PipelineData.OperationsAsync.Any())
        return OnOperationsEmpty(context);
      return OnOperationProcessingComplete(context.PipelineData.OperationsAsync) ?? PipelineContinuation.Continue;
    }

    private IEnumerable<IOperationAsync> ProcessOperations(IEnumerable<IOperationAsync> operations)
    {
      var chain = GetMethods().Chain();
      return chain == null ? new IOperationAsync[0] : chain(operations);
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationFiltering>();
    }

    static PipelineContinuation? OnOperationProcessingComplete(IEnumerable<IOperationAsync> ops)
    {
      return null;
    }


    private IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
    {
      var operationProcessors = _resolver.ResolveAll<IOperationCodecSelector>();

      foreach (var filter in operationProcessors)
        yield return filter.Process;
    }
  }
}