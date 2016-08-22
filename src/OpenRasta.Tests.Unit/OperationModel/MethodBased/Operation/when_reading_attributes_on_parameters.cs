using System.Linq;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_reading_attributes_on_parameters : operation_context<OperationHandlerForAttributes>
  {
    [Test]
    public void an_attribute_not_defined_returns_null()
    {
      given_operation("GetHasParameterAttribute", typeof(int));

      Operation.Inputs.First().Binder.ShouldBeAssignableTo<ParameterBinder>();
    }
  }
}
