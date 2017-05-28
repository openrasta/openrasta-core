using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestCodecSelector : KnownStages.ICodecRequestSelection
  {
    readonly Func<IEnumerable<IOperationCodecSelector>> _requestCodecs;

    public RequestCodecSelector(Func<IEnumerable<IOperationCodecSelector>> requestCodecs)
    {
      _requestCodecs = requestCodecs;
    }

    PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.RequestMediaTypeUnsupported();
      return PipelineContinuation.RenderNow;
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      context.PipelineData.OperationsAsync =
        ProcessOperations(context.PipelineData.OperationsAsync).ToList();

      return !context.PipelineData.OperationsAsync.Any()
        ? OnOperationsEmpty(context)
        : PipelineContinuation.Continue;
    }

    IEnumerable<IOperationAsync> ProcessOperations(IEnumerable<IOperationAsync> operations)
    {
      var chain = GetMethods().Chain();
      return chain == null ? new IOperationAsync[0] : chain(operations);
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationFiltering>();
    }

    IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
    {
      foreach (var filter in _requestCodecs())
        yield return filter.Process;
    }
  }
}