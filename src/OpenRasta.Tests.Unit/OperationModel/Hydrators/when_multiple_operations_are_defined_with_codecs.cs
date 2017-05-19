using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.Testing;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_multiple_operations_are_defined_with_codecs : request_entity_reader_context
  {
    [Test]
    public void ambiguous_calls_get_rejected()
    {
      given_filter();
      given_operations();
      given_operation_has_codec_match<ApplicationOctetStreamCodec>("PostName", MediaType.Xml, 1.0f);
      given_operation_has_codec_match<ApplicationXWwwFormUrlencodedKeyedValuesCodec>("PostAddress", MediaType.Xml, 1.0f);

      when_filtering_operations();

      Error.ShouldBeAssignableTo<AmbiguousRequestException>();
    }

    [Test]
    public void the_one_with_the_highest_score_is_selected()
    {
      given_filter();
      given_operations();
      given_operation_has_codec_match<ApplicationOctetStreamCodec>("PostName", MediaType.Xml, 0.5f);
      given_operation_has_codec_match<ApplicationXWwwFormUrlencodedKeyedValuesCodec>("PostAddress", MediaType.Xml, 1.0f);

      when_filtering_operations();

      ResultOperation.GetRequestCodec().CodecRegistration.CodecType.ShouldBe(typeof(ApplicationXWwwFormUrlencodedKeyedValuesCodec));
    }
    [Test]
    public void the_one_without_a_codec_is_not_selected()
    {
      given_filter();
      given_operations();

      given_operation_has_codec_match<ApplicationOctetStreamCodec>("PostName", MediaType.Xml, 1.0f);

      when_filtering_operations();

      ResultOperation.GetRequestCodec().CodecRegistration.CodecType.ShouldBe(typeof(ApplicationOctetStreamCodec));
    }
  }
}
