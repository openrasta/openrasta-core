using NUnit.Framework;
using OpenRasta.Testing;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_no_operation_can_be_processed : request_entity_reader_context
  {
    [Test]
    public void no_operation_gets_selected()
    {
      given_filter();
      given_operations();

      when_filtering_operations();

      ResultOperation.ShouldBeNull();

    }
  }
}
