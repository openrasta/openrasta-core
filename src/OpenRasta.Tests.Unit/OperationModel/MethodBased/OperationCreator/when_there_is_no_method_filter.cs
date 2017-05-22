using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
  public class when_there_is_no_method_filter : method_based_operation_creator_context
  {
    [Test]
    public void by_default_operations_are_created_for_all_public_instance_and_static_methods()
    {
      given_operation_creator(null);
      given_handler<MockHandler>();

      when_creating_operations();

      then_operation_count_should_be_same_as_public_methods_on_handler(typeof(MockHandler));
    }
    void then_operation_count_should_be_same_as_public_methods_on_handler(Type handlerType)
    {
      Operations.Count().ShouldBe(handlerType.GetMethods(BindingFlags.Instance |
                                                         BindingFlags.Static |
                                                         BindingFlags.Public |
                                                         BindingFlags.FlattenHierarchy).Length);
      //return valueToAnalyse;
    }
  }
}