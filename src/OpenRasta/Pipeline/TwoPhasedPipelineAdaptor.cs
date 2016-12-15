using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.Pipeline.CallGraph;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
    public class TwoPhasedPipelineAdaptor : IPipeline, IPipelineAsync
    {
        readonly IGenerateCallGraphs _graphs;
        Func<ICommunicationContext, Task> _invoker;
        public bool IsInitialized { get; }
        public IList<IPipelineContributor> Contributors { get; }
        public IEnumerable<ContributorCall> CallGraph { get; private set; }

        public TwoPhasedPipelineAdaptor(IDependencyResolver resolver)
        {
            _graphs = resolver.HasDependency<IGenerateCallGraphs>()
                ? resolver.Resolve<IGenerateCallGraphs>()
                : new WeightedCallGraphGenerator();
            Contributors = resolver.ResolveAll<IPipelineContributor>()
                .ToList()
                .AsReadOnly();
        }

        public void Initialize()
        {
            Initialize(true);
        }

        public void Initialize(bool validate)
        {
            if (validate) Contributors.VerifyKnownStagesRegistered();

            _invoker = (CallGraph = _graphs.GenerateCallGraph(Contributors))
                .ToTwoPhasedMiddleware<KnownStages.IOperationResultInvocation>()
                .Invoke;
        }

        public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> notification)
        {
            throw new NotImplementedException("Should never be called here, ever!");
        }

        public void Run(ICommunicationContext context)
        {
            RunAsync(context).GetAwaiter().GetResult();
        }

        public Task RunAsync(ICommunicationContext env)
        {
            if (env.PipelineData.PipelineStage == null)
                env.PipelineData.PipelineStage = new PipelineStage(this);
            return _invoker(env);
        }

        public IPipelineExecutionOrder Use(Func<ICommunicationContext, Task<PipelineContinuation>> action)
        {
            throw new NotImplementedException("Should never be called here, ever!");
        }

        public IPipelineExecutionOrder Notify(Func<ICommunicationContext, Task> action)
        {
            throw new NotImplementedException("Should never be called here, ever!");
        }
    }
}