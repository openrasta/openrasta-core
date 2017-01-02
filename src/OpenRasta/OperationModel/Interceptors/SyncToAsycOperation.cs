using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenRasta.OperationModel.Interceptors
{
  public class SyncToAsycOperation : IOperationAsync
  {
    readonly IOperation _inner;

    public SyncToAsycOperation(IOperation inner)
    {
      _inner = inner;
    }

    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();
    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();
    public IEnumerable<InputMember> Inputs => _inner.Inputs;
    public IDictionary ExtendedProperties => _inner.ExtendedProperties;
    public string Name => _inner.Name;
    public override string ToString() => _inner.ToString();

    public IEnumerable<OutputMember> Invoke()
    {
      return _inner.Invoke();
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      return Task.FromResult(_inner.Invoke());
    }
  }
}