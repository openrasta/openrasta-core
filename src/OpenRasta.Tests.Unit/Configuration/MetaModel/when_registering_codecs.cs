using System;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Web;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registering_codecs : configuration_context
  {
    [Test]
    public void can_add_a_codec_by_type()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy<CustomerCodec>();

        FirstRegistration.Codecs[0].CodecType.ShouldBe(typeof(CustomerCodec));
      });
    }

    [Test]
    public void can_add_a_codec_by_type_instance()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy(typeof(CustomerCodec));

        FirstRegistration.Codecs[0].CodecType.ShouldBe(typeof(CustomerCodec));
      });
    }

    [Test]
    public void can_add_a_codec_configuration()
    {
      ExecuteTest(parent =>
      {
        var configurationObject = new object();
        parent.TranscodedBy(typeof(CustomerCodec), configurationObject);

        FirstRegistration.Codecs[0].Configuration.ShouldBe(configurationObject);
      });
    }

    [Test]
    public void can_add_a_specific_media_type_for_a_codec()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy<CustomerCodec>().ForMediaType(MediaType.Javascript.ToString());

        FirstRegistration.Codecs[0].MediaTypes[0].MediaType.ShouldBe(MediaType.Javascript);
      });
    }

    [Test]
    public void can_register_two_media_types()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy<CustomerCodec>().ForMediaType("application/xhtml+xml").ForMediaType("text/plain");

        FirstRegistration.Codecs[0].MediaTypes[0].MediaType.ToString().ShouldBe(
            "application/xhtml+xml");
        FirstRegistration.Codecs[0].MediaTypes[1].MediaType.ToString().ShouldBe( "text/plain");
      });
    }

    [Test]
    public void can_register_an_extension_on_mediatype()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy<CustomerCodec>().ForMediaType("application/xhtml+xml").ForExtension("xml");

        FirstRegistration.Codecs[0].MediaTypes[0].Extensions.ShouldBe(new[] {"xml"});
      });
    }

    [Test]
    public void can_register_multiple_extensions_on_multiple_mediatypes()
    {
      ExecuteTest(parent =>
      {
        parent.TranscodedBy<CustomerCodec>()
            .ForMediaType(MediaType.Xhtml)
            .ForExtension("xml")
            .ForExtension("xhtml")
            .ForMediaType("text/html")
            .ForExtension("html");

        FirstRegistration.Codecs[0]
            .MediaTypes[0]
            .Extensions.ShouldBe(new[] {"xml", "xhtml"});

        FirstRegistration.Codecs[0]
            .MediaTypes[1]
            .Extensions.ShouldBe(new[] {"html"});
      });
    }

    [Test]
    public void can_register_multiple_codecs_with_multiple_media_types_and_multiple_extensions()
    {
      ExecuteTest(parent =>
      {
        parent
            .TranscodedBy<CustomerReaderCodec>()
            .And
            .TranscodedBy<CustomerCodec>()
            .ForMediaType("application/xhtml+xml")
            .ForExtension("xml")
            .ForExtension("xhtml")
            .And
            .TranscodedBy<CustomerWriterCodec>()
            .ForMediaType("application/unknown");

        FirstRegistration.Codecs[0].CodecType.ShouldBe(typeof(CustomerReaderCodec));

        FirstRegistration.Codecs[1].CodecType.ShouldBe(typeof(CustomerCodec));
        FirstRegistration.Codecs[1]
            .MediaTypes[0]
            .Extensions.ShouldBe(new[] {"xml", "xhtml"});

        FirstRegistration.Codecs[2].CodecType.ShouldBe(typeof(CustomerWriterCodec));
        FirstRegistration.Codecs[2]
            .MediaTypes[0]
            .Extensions.ShouldBeEmpty();
      });
    }

    [Test]
    public void cannot_register_codec_not_implementing_icodec()
    {
      Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().WithoutUri.TranscodedBy(typeof(string)))
          .ShouldThrow<ArgumentOutOfRangeException>();
    }

    void ExecuteTest(Action<ICodecParentDefinition> test)
    {
      test(ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy<CustomerHandler>());
      MetaModel.ResourceRegistrations.Clear();
      test(ResourceSpaceHas.ResourcesOfType<Frodo>().WithoutUri);
    }
  }
}