using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Testing;
using OpenRasta.Tests.Unit.Configuration;
using OpenRasta.TypeSystem;
using Enumerable = System.Linq.Enumerable;

namespace LegacyManualConfiguration_Specification
{
    public class when_configuring_codecs : configuration_context
    {
        CodecRegistration ThenTheCodecFor<TResource, TCodec>(string mediaType)
        {
            return
                    Enumerable.SingleOrDefault<CodecRegistration>(DependencyManager.Codecs.Where(codec => codec.ResourceType.CompareTo(TypeSystems.Default.FromClr(typeof (TResource)))==0 && codec.CodecType == typeof (TCodec) && codec.MediaType.MediaType == mediaType).
                                                                                  Distinct());
        }

        [MediaType("application/vnd.rasta.test")]
        class Codec : NakedCodec
        {
        }

        [MediaType("application/vnd.rasta.test1")]
        [MediaType("application/vnd.rasta.test2")]
        class MultiCodec : NakedCodec
        {
        }

        class NakedCodec : ICodec
        {
            public object Configuration { get; set; }
        }

        [Test]
        public void a_codec_registered_with_configuration_media_type_doesnt_have_the_attribute_media_type_registered()
        {
            given_resource<Customer>("/customer")
                    .HandledBy<CustomerHandler>()
                    .AndTranscodedBy<Codec>()
                    .ForMediaType("application/vnd.rasta.custom");

            WhenTheConfigurationIsFinished();

            ThenTheCodecFor<Customer, Codec>("application/vnd.rasta.test")
                    .ShouldBeNull();
            ThenTheCodecFor<Customer, Codec>("application/vnd.rasta.custom")
                    .ShouldNotBeNull();
        }

        [Test]
        public void a_codec_registered_with_two_media_type_attributes_is_registered_twice()
        {
            given_resource<Customer>("/customer")
                    .HandledBy<CustomerHandler>()
                    .AndTranscodedBy<MultiCodec>();

            WhenTheConfigurationIsFinished();

            ThenTheCodecFor<Customer, MultiCodec>("application/vnd.rasta.test1")
                    .ShouldNotBeNull();
            ThenTheCodecFor<Customer, MultiCodec>("application/vnd.rasta.test2")
                    .ShouldNotBeNull();
        }

        [Test]
        public void a_codec_registered_with_two_media_types_in_configuration_is_registered_twice()
        {
            given_resource<Customer>("/customer")
                    .HandledBy<CustomerHandler>()
                    .AndTranscodedBy<Codec>()
                    .ForMediaType("application/vnd.rasta.config1")
                    .AndForMediaType("application/vnd.rasta.config2");

            WhenTheConfigurationIsFinished();

            ThenTheCodecFor<Customer, Codec>("application/vnd.rasta.config1")
                    .ShouldNotBeNull();
            ThenTheCodecFor<Customer, Codec>("application/vnd.rasta.config2")
                    .ShouldNotBeNull();
        }

        [Test]
        public void a_codec_registered_without_media_types_is_registered_with_the_default_attributed_media_types()
        {
            given_resource<Customer>("/customer")
                    .HandledBy<CustomerHandler>()
                    .AndTranscodedBy<Codec>();

            WhenTheConfigurationIsFinished();

            ThenTheCodecFor<Customer, Codec>("application/vnd.rasta.test")
                    .ShouldNotBeNull();
        }

        [Test]
        public void registering_a_codec_without_media_type_in_config_or_in_attributes_raises_an_error()
        {
            given_resource<Customer>("/customer")
                    .HandledBy<CustomerHandler>()
                    .AndTranscodedBy<NakedCodec>();

            Executing(WhenTheConfigurationIsFinished)
                    .ShouldThrow<OpenRastaConfigurationException>();
        }
    }
}