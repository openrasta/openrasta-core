using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncMethod<T> : AbstractMethodOperation, IOperationAsync
  {
    public IDictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null, IDependencyResolver resolver = null)
      : base(method, binderLocator)
    {
      Resolver = resolver;
    }

    public async Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      var task = (Task<T>) Method.Invoke(instance, parameters).Single();
      var result = await task;
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