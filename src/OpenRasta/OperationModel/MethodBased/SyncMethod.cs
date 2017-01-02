using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public class SyncMethod : AbstractMethodOperation, IOperation
  {
    public SyncMethod(IMethod method, IObjectBinderLocator binderLocator = null)
      : base(method, binderLocator)
    {
    }

    public IEnumerable<OutputMember> Invoke()
    {
      var instance = CreateInstance(OwnerType, Resolver);
      var parameters = GetParameters();
      return new[]
      {
        new OutputMember
        {
          Member = Method.OutputMembers.Single(),
          Value = Method.Invoke(instance, parameters)
        }
      };
    }
  }
}