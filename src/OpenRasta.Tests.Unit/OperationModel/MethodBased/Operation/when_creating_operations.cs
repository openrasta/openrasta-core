using System;
using NUnit.Framework;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_creating_operations : operation_context<MockOperationHandler>
  {
    [Test]
    public void the_operation_name_is_the_method_name()
    {
      given_operation("Get", typeof(int));

      ShouldBeTestExtensions.ShouldBe(Operation.Name, "Get");
      //return valueToAnalyse;
    }
    [Test]
    public void the_operation_string_representation_is_the_method_signature()
    {
      given_operation("Get", typeof(int));

      ShouldBeTestExtensions.ShouldBe(Operation.ToString(), "MockOperationHandler::Get(Int32 index)");
      //return valueToAnalyse;
    }
    [Test]
    public void property_getters_are_not_selected()
    {
      Executing(()=>given_operation("get_Dependency")).ShouldThrow<InvalidOperationException>();


    }
  }
}
