using System.Collections.Generic;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
    public class RequestCodecSelector
        : AbstractOperationProcessing<IOperationCodecSelector, KnownStages.ICodecRequestSelection>,
          KnownStages.ICodecRequestSelection
    {
        public RequestCodecSelector(IDependencyResolver resolver) : base(resolver)
        {
        }

        protected override void InitializeWhen(IPipelineExecutionOrder pipeline)
        {
            pipeline.After<KnownStages.IOperationFiltering>();
        }
        protected override PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
        {
            context.OperationResult = new OperationResult.RequestMediaTypeUnsupported();
            return PipelineContinuation.RenderNow;
        }
    }
}
