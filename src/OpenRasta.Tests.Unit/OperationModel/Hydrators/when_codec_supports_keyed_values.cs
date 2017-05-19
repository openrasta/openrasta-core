using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.Testing;
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
      given_filter();
      given_operations();
      given_operation_has_codec_match<ApplicationXWwwFormUrlencodedKeyedValuesCodec>("PostName", MediaType.Xml, 1.0f);
      given_request_entity_body("Frodo.LastName=Baggins&Frodo.Unknown=avalue");

      when_entity_is_read();

      ShouldBeTestExtensions.ShouldBe(ResultOperation.Inputs.Required().First().Binder.BuildObject()
          .Instance.ShouldBeAssignableTo<Frodo>()
          .LastName, "Baggins");
      //return valueToAnalyse;
    }
  }
}