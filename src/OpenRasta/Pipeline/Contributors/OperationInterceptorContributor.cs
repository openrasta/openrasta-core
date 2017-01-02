using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
    public class OperationInterceptorContributor : IPipelineContributor
    {
        readonly IDependencyResolver _resolver;

        public OperationInterceptorContributor(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(WrapOperations)
                .After<KnownStages.IRequestDecoding>()
                .And
                .Before<KnownStages.IOperationExecution>();
        }

        PipelineContinuation WrapOperations(ICommunicationContext context)
        {
            context.PipelineData.Operations = from op in context.PipelineData.Operations.Select(ConvertToAsync)
                                              let interceptors = _resolver.Resolve<IOperationInterceptorProvider>().GetInterceptors(op)
                                              select (IOperationAsync)new SyncOperationWithInterceptors(op, interceptors);

            return PipelineContinuation.Continue;
        }

      IOperationAsync ConvertToAsync(IOperation arg)
      {
        return arg as IOperationAsync ?? new LeagacySyncOperation(arg);
      }
    }

  internal class LeagacySyncOperation : IOperationAsync
  {
    readonly IOperation _impl;

    public LeagacySyncOperation(IOperation operation)
    {
      _impl = operation;
    }
    public T FindAttribute<T>() where T : class => _impl.FindAttribute<T>();
    public IEnumerable<T> FindAttributes<T>() where T : class => _impl.FindAttributes<T>();
    public IEnumerable<InputMember> Inputs => _impl.Inputs;
    public IDictionary ExtendedProperties => _impl.ExtendedProperties;
    public string Name => _impl.Name;
    public IEnumerable<OutputMember> Invoke() => _impl.Invoke();
    public Task<IEnumerable<OutputMember>> InvokeAsync() => Task.FromResult(Invoke());
  }
}
