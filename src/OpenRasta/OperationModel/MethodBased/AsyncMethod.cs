using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncMethod : AbstractMethodOperation, IOperationAsync
  {
    public IDictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null, IDependencyResolver resolver = null)
      : base(method, binderLocator, resolver)
    {
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      return ((Task) Method.Invoke(instance, parameters))
        .ContinueWith(task => Enumerable.Empty<OutputMember>());
    }
  }

  public class AsyncMethod<T> : AbstractMethodOperation, IOperationAsync
  {
    public IDictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null, IDependencyResolver resolver = null)
      : base(method, binderLocator, resolver)
    {
    }

    public async Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      var result = await (Task<T>) Method.Invoke(instance, parameters);
      return new[]
      {
        new OutputMember
        {
          Member = Method.OutputMembers.Single(),
          Value = result
        }
      };
    }
  }
}