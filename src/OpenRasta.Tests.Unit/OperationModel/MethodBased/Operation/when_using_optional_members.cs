using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.TypeSystem;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
    public class when_using_optional_members : operation_context<MockOperationHandler>
    {
        [Test]
        public void the_operation_is_ready_for_invocation()
        {
            given_operation("Get", typeof(int));

          Operation.Inputs.AllReady().ShouldBeTrue();
        }
        [Test]
        public void all_parameters_are_satisfied()
        {
            given_operation("Get", typeof(int));

          Operation.Inputs.CountReady().ShouldBe(1);
          //return valueToAnalyse;
        }
        [Test]
        public void a_default_parameter_value_is_supported()
        {
            given_operation("Search",typeof(string));

          Operation.Inputs.Optional().First().IsOptional.ShouldBeTrue();
          Operation.Inputs.Optional().First().Member.ShouldBeAssignableTo<IParameter>().DefaultValue.ShouldBe("*");
          //return valueToAnalyse;
        }
    }

  public class when_using_native_optional_members : operation_context<MockOperationHandler>
  {

    [Test]
    public void the_operation_is_ready_for_invocation()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.AllReady().ShouldBeTrue();
    }

    [Test]
    public void all_parameters_are_satisfied()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.CountReady().ShouldBe(1);
      //return valueToAnalyse;
    }

    [Test]
    public void a_default_parameter_value_is_supported()
    {
      given_operation("SearchNative", typeof(string));

      Operation.Inputs.Optional().First().IsOptional.ShouldBeTrue();
      Operation.Inputs.Optional().First().Member.ShouldBeAssignableTo<IParameter>().DefaultValue.ShouldBe("*");
      //return valueToAnalyse;
    }
  }
}
