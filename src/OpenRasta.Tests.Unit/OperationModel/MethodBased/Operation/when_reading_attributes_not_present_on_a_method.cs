using System;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_reading_attributes_not_present_on_a_method : operation_context<OperationHandlerForAttributes>
  {
    [Test]
    public void an_attribute_not_defined_returns_null()
    {
      given_operation("GetHasOneAttribute", typeof(int));

      Operation.FindAttribute<AttributeUsageAttribute>().LegacyShouldBeNull();
    }
    [Test]
    public void an_attribute_not_defined_returns_an_empty_collection()
    {
      given_operation("GetHasOneAttribute", typeof(int));

      Operation.FindAttributes<AttributeUsageAttribute>().legacyShouldNotBeNull().ShouldBeEmpty();
    }
  }
}
