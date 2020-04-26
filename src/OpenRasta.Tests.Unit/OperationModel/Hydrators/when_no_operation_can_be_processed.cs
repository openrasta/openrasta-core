using NUnit.Framework;
using OpenRasta.OperationModel.Hydrators;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_no_operation_can_be_processed : request_entity_reader_context
  {
    [Test]
    public void no_operation_gets_selected()
    {
      given_entity_reader();
      given_operations_for<HandlerRequiringInputs>();

      when_filtering_operations();

      ReadResult.ShouldBe(RequestReadResult.NoneFound);
      ResultOperation.ShouldBeNull();
    }
  }
}
