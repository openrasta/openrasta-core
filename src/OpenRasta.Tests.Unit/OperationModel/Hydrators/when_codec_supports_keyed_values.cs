using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_codec_supports_keyed_values : request_entity_reader_context
  {
    [Test]
    public void the_keyed_values_are_used_to_build_the_parameter()
    {
      given_entity_reader();
      given_operations_for<HandlerRequiringInputs>();
      given_operation_has_codec_match<ApplicationXWwwFormUrlencodedKeyedValuesCodec>("PostName", MediaType.Xml, 1.0f);
      given_request_entity_body("Frodo.LastName=Baggins&Frodo.Unknown=avalue");

      when_filtering_operations();

      ResultOperation.Inputs.Required().First().Binder.BuildObject()
          .Instance.ShouldBeAssignableTo<Frodo>()
          .LastName.ShouldBe( "Baggins");
    }
  }
}