using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class AsyncMethod : AbstractMethodOperation, IOperationAsync
  {
    public AsyncMethod(IMethod method, IObjectBinderLocator binderLocator = null)
      : base(method, binderLocator)
    {
    }

    public Task<IEnumerable<OutputMember>> InvokeAsync()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();

      return ((Task) Method.Invoke(instance, parameters).Single())
        .ContinueWith(task => Enumerable.Empty<OutputMember>());
    }

    public IEnumerable<OutputMember> Invoke()
    {
      throw new NotImplementedException();
    }

    public static IOperationAsync FromType(Type methodType, IMethod method, IObjectBinderLocator binderLocator = null)
    {
      return (IOperationAsync) Activator.CreateInstance(
        typeof(AsyncMethod<>)
          .MakeGenericType(method.OutputMembers.Single().StaticType.GenericTypeArguments),
        method, binderLocator);
    }
  }
}