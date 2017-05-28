using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
    public abstract class AbstractOperationProcessing<TProcessor, TStage> : IPipelineContributor
        where TProcessor : IOperationProcessor<TStage>
        where TStage : IPipelineContributor
    {
        readonly IDependencyResolver _resolver;

        protected AbstractOperationProcessing(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public virtual PipelineContinuation ProcessOperations(ICommunicationContext context)
        {
            context.PipelineData.OperationsAsync = ProcessOperations(context.PipelineData.OperationsAsync).ToList();
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

        protected abstract void InitializeWhen(IPipelineExecutionOrder pipeline);

        protected virtual PipelineContinuation? OnOperationProcessingComplete(IEnumerable<IOperationAsync> ops)
        {
            return null;
        }

        protected virtual PipelineContinuation OnOperationsEmpty(ICommunicationContext context)
        {
            context.OperationResult = new OperationResult.MethodNotAllowed();

            return PipelineContinuation.RenderNow;
        }

        IEnumerable<Func<IEnumerable<IOperationAsync>, IEnumerable<IOperationAsync>>> GetMethods()
        {
          // todo func injection
            var operationProcessors = _resolver.ResolveAll<TProcessor>();

            foreach (var filter in operationProcessors)
                yield return filter.Process;
        }
    }
}