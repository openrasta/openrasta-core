using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncMethod<T> : AbstractMethodOperation, IOperationAsync
  {
    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null)
      : base(method, binderLocator)
    {
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      return ((Task<T>) Method.Invoke(instance, parameters).Single())
        .ContinueWith(task => (IEnumerable<OutputMember>) new[]
        {
          new OutputMember
          {
            Member = Method.OutputMembers.Single(),
            Value = task.Result
          }
        });
    }

    public IEnumerable<OutputMember> Invoke()
    {
      throw new NotImplementedException();
    }
  }
}