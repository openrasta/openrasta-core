using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.Testing;
using OpenRasta.Web;
using OpenRasta.Web.Codecs;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public class when_codec_supports_object_building : request_entity_reader_context
  {
    [Test]
    public void the_object_is_built()
    {
      given_filter();
      given_operations();
      given_operation_has_codec_match<ApplicationOctetStreamCodec>("PostStream", MediaType.ApplicationOctetStream, 1.0f);
      given_request_entity_body(new byte[]{0});

      when_entity_is_read();

      ResultOperation.Name.LegacyShouldBe("PostStream");
      ResultOperation.Inputs.Required().First().Binder.BuildObject()
        .Instance.ShouldBeAssignableTo<Stream>()
        .ReadByte().LegacyShouldBe(0);
    }
    [Test]
    public void an_error_is_collected_if_codec_raises_an_error()
    {

      given_filter();
      given_operations();
            
      given_operation_has_codec_match<XmlDataContractCodec>("PostName", MediaType.Xml, 1.0f);
      given_request_entity_body(new byte[] { 0 });

      when_filtering_operations();
      ResultOperation.Name.LegacyShouldBe("PostName");
      Errors.Errors.LegacyShouldHaveCountOf(1);
    }
  }
}
