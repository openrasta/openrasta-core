using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.OperationModel.Interceptors;

namespace OpenRasta.OperationModel
{
  public class AsyncOperationWithInterceptors : IOperationAsync
  {
    readonly IOperationAsync _inner;
    static readonly Func<IOperationAsync, Task<IEnumerable<OutputMember>>> IdentityInterceptor = op => op.InvokeAsync();
    readonly Func<IOperationAsync, Task<IEnumerable<OutputMember>>> _invoker;

    public AsyncOperationWithInterceptors(IOperationAsync inner)
    {
      _inner = inner;
      _invoker = AsyncInvoker(inner);
    }

    static Func<IOperationAsync, Task<IEnumerable<OutputMember>>> AsyncInvoker(IOperationAsync operation)
    {
      return operation.FindAttributes<IOperationInterceptorAsync>()
        .Aggregate(IdentityInterceptor, (next, interceptor) => interceptor.Compose(next));
    }

    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();

    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();

    public IEnumerable<InputMember> Inputs => _inner.Inputs;

    public IDictionary<string, object> ExtendedProperties => _inner.ExtendedProperties;

    public string Name => _inner.Name;

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      return _invoker(_inner);
    }
  }
}