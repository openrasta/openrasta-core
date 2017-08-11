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
    private Func<IEnumerable<IOperationCodecSelector>> _codecSelectors;

    public RequestCodecSelector(IDependencyResolver resolver)
    {
      _resolver = resolver;
      _codecSelectors = ()=>_resolver.ResolveAll<IOperationCodecSelector>();
    }

    private static PipelineContinuation RequestMediaTypeUnsupported(ICommunicationContext context)
    {
      context.OperationResult = new OperationResult.RequestMediaTypeUnsupported();
      return PipelineContinuation.RenderNow;
    }

    private PipelineContinuation ProcessOperations(ICommunicationContext context)
    {
      var ops = ProcessOperations(context.PipelineData.OperationsAsync.ToList());
      context.PipelineData.OperationsAsync = ops;
      return !ops.Any() ? RequestMediaTypeUnsupported(context) : PipelineContinuation.Continue;
    }

    private List<IOperationAsync> ProcessOperations(List<IOperationAsync> operations)
    {
      return _codecSelectors()
        .Aggregate(
          operations,
          (list, selector) => selector.Process(list).ToList());
    }

    public virtual void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessOperations).After<KnownStages.IOperationFiltering>();
    }
  }
}