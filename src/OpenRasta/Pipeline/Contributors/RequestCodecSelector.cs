using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  public class RequestCodecSelector
    : KnownStages.ICodecRequestSelection
  {
    readonly Func<IEnumerable<IOperationCodecSelector>> _codecSelectors;

    public RequestCodecSelector(Func<IEnumerable<IOperationCodecSelector>> codecs)
    {
      _codecSelectors = codecs;
    }
    static PipelineContinuation RequestMediaTypeUnsupported(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.RequestMediaTypeUnsupported();
      return PipelineContinuation.RenderNow;
    }

    PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      var ops = ProcessOperations(context.PipelineData.OperationsAsync.ToList());
      context.PipelineData.OperationsAsync = ops;
      return !ops.Any() ? RequestMediaTypeUnsupported(context) : PipelineContinuation.Continue;
    }

    List<IOperationAsync> ProcessOperations(List<IOperationAsync> operations)
    {
      return _codecSelectors()
        .Aggregate(
          operations,
          (list, selector) => selector.Process(list).ToList());
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationFiltering>();
    }
  }
}