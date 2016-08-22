using System.Linq;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased.Operation
{
  public class when_reading_attributes_present_on_a_method : operation_context<OperationHandlerForAttributes>
  {
    [Test]
    public void a_single_attribute_is_found()
    {
      given_operation("GetHasOneAttribute", typeof(int));

      Operation.FindAttribute<DescriptionAttribute>()
        .ShouldNotBeNull()
        .Description.ShouldBe("Description");
    }
    [Test]
    public void multile_attributes_are_found()
    {
      given_operation("GetHasTwoAttributes",typeof(int));

      var attributes = Operation.FindAttributes<UselessAttribute>();
      attributes
        .FirstOrDefault(x => x.Name == "one").ShouldNotBeNull();
      attributes
        .FirstOrDefault(x => x.Name == "two").ShouldNotBeNull();
    }
    [Test]
    public void attributes_on_the_metod_owner_type_are_returned()
    {
      given_operation("GetHasTwoAttributes", typeof(int));

      var attributes = Operation.FindAttributes<UselessAttribute>();
      attributes
        .FirstOrDefault(x => x.Name == "type attribute").ShouldNotBeNull();
    }
    [Test]
    public void an_attribute_can_be_found_when_searching_by_interface()
    {
      given_operation("GetHasTwoAttributes", typeof(int));
      Operation.FindAttributes<IUseless>()
        .Count().ShouldBe(3);

    }
  }
}
