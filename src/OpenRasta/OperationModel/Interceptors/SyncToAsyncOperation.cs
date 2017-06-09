#pragma warning disable 618
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenRasta.OperationModel.Interceptors
{
  public class SyncToAsyncOperation : IOperationAsync, IOperation
  {
    readonly IOperation _inner;

    public SyncToAsyncOperation(IOperation inner)
    {
      _inner = inner;
      ExtendedProperties = new DictionaryAdapter(inner.ExtendedProperties)
      {
        {"async", true},
      };
    }

    public T FindAttribute<T>() where T : class => _inner.FindAttribute<T>();
    public IEnumerable<T> FindAttributes<T>() where T : class => _inner.FindAttributes<T>();
    public IEnumerable<InputMember> Inputs => _inner.Inputs;
    public IDictionary<string, object> ExtendedProperties { get; }
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

    IDictionary IOperation.ExtendedProperties => _inner.ExtendedProperties;
  }
}

#pragma warning restore 618