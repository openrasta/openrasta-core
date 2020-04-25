using System;
using System.Linq;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;

namespace OpenRasta.Tests.Unit.Infrastructure
{
  public abstract class operation_context<THandler> : openrasta_context
  {
    protected operation_context()
    {
      Handler = TypeSystem.FromClr<THandler>();
    }

    protected IType Handler { get; }
    protected IOperationAsync Operation { get; set; }

    protected void given_operation(string name, params Type[] parameters)
    {
      var method = (from m in Handler.GetMethods()
        where m.InputMembers.Count() == parameters.Length && m.Name.EqualsOrdinalIgnoreCase(name)
        let matchingParams =
        (from parameter in m.InputMembers
          from typeParameter in parameters
          where parameter.Type.CompareTo(parameter.TypeSystem.FromClr(typeParameter)) == 0
          select parameter).Count()
        where parameters.Length == 0 || matchingParams == parameters.Length
        select m).First();
      if (method.Owner != Handler)
        throw new InvalidOperationException();
      Operation = MethodBasedOperationCreator.CreateOperationDescriptor(Handler,method).Create();
    }
  }
}