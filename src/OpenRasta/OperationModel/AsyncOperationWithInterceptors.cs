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

    public AsyncOperationWithInterceptors(IOperationAsync inner, IEnumerable<IOperationInterceptorAsync> systemInterceptors)
    {
      _inner = inner;
      _invoker = AsyncInvoker(inner, systemInterceptors);
    }

    static Func<IOperationAsync, Task<IEnumerable<OutputMember>>> AsyncInvoker(IOperationAsync operation, IEnumerable<IOperationInterceptorAsync> systemInterceptors)
    {
      return operation
        .FindAttributes<IOperationInterceptorAsync>()
        .Concat(systemInterceptors)
        .Aggregate(IdentityInterceptor, (next, interceptor) => interceptor.Compose(next));
    }

    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();

    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();

    public IEnumerable<InputMember> Inputs => _inner.Inputs;

    public IDictionary<string, object> ExtendedProperties => _inner.ExtendedProperties;

    public string Name => _inner.Name;

    public override string ToString() => _inner.ToString();

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      return _invoker(_inner);
    }
  }
}