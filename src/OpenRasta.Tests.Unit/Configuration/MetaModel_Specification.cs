using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Testing;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;
using OpenRasta.Web;
using OpenRasta.Web.UriDecorators;
using TypeSystems = OpenRasta.TypeSystem.TypeSystems;

namespace Configuration_Specification
{
    public class configuration_context : context
    {
        protected static IHas ResourceSpaceHas;
        protected static IUses ResourceSpaceUses;
        protected IMetaModelRepository MetaModel;
        protected IDependencyResolver Resolver;

        protected ResourceModel FirstRegistration
        {
            get { return MetaModel.ResourceRegistrations.First(); }
        }

        protected override void SetUp()
        {
            base.SetUp();
            Resolver = new InternalDependencyResolver();
            Resolver.AddDependency<ITypeSystem, ReflectionBasedTypeSystem>();
            MetaModel = new MetaModelRepository(Resolver);
            ResourceSpaceHas = new FluentTarget(Resolver, MetaModel);
            ResourceSpaceUses = new FluentTarget(Resolver, MetaModel);

            DependencyManager.SetResolver(Resolver);
        }

        protected override void TearDown()
        {
            base.TearDown();
            DependencyManager.UnsetResolver();
        }
    }

    public class when_registering_resources : configuration_context
    {
        [Test]
        public void a_resource_by_generic_type_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType<Customer>();

            MetaModel.ResourceRegistrations.Count.LegacyShouldBe(1);
            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBe(typeof(Customer));
        }

        [Test]
        public void a_resource_by_name_is_registered()
        {
            ResourceSpaceHas.ResourcesNamed("customers");

            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBe("customers");
        }

        [Test]
        public void a_resource_by_type_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType(typeof(Customer));

            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBe(typeof(Customer));
        }

        [Test]
        public void a_resource_with_any_key_is_registered()
        {
            var key = new object();
            ResourceSpaceHas.ResourcesWithKey(key);

            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBeTheSameInstanceAs(key);
        }

        [Test]
        public void a_resource_with_IType_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType(TypeSystems.Default.FromClr(typeof(Customer)));

            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBeOfType<IType>().Name.LegacyShouldBe("Customer");
        }

        [Test]
        public void cannot_execute_registration_on_null_IHas()
        {
            Executing(() => ((IHas)null).ResourcesWithKey(new object()))
                .LegacyShouldThrow<ArgumentNullException>();
        }
        [Test]
        public void a_resource_of_type_strict_is_registered_as_a_strict_type()
        {
            ResourceSpaceHas.ResourcesOfType<Strictly<Customer>>();

            MetaModel.ResourceRegistrations[0].ResourceKey.LegacyShouldBeOfType<Type>().LegacyShouldBe<Customer>();
            MetaModel.ResourceRegistrations[0].IsStrictRegistration.LegacyShouldBeTrue();

        }
        [Test]
        public void cannot_register_a_resource_with_a_null_key()
        {
            Executing(() => ResourceSpaceHas.ResourcesWithKey(null))
                .LegacyShouldThrow<ArgumentNullException>();
        }
    }

    public class when_registering_uris : configuration_context
    {
        IList<UriModel> TheUris
        {
            get { return MetaModel.ResourceRegistrations[0].Uris; }
        }

        [Test]
        public void a_null_language_defaults_to_the_inviariant_culture()
        {
            ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").InLanguage(null);
            TheUris[0].Language.LegacyShouldBe(CultureInfo.InvariantCulture);
        }

        [Test]
        public void a_resource_can_be_registered_with_no_uri()
        {
            ICodecParentDefinition reg = ResourceSpaceHas.ResourcesOfType<Customer>().WithoutUri;
            TheUris.Count.LegacyShouldBe(0);
        }

        [Test]
        public void a_uri_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer");

            TheUris.Count.LegacyShouldBe(1);
            TheUris[0].Uri.LegacyShouldBe("/customer");
        }

        [Test]
        public void a_uri_language_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").InLanguage("fr");
            TheUris[0].Language.Name.LegacyShouldBe("fr");
        }

        [Test]
        public void a_uri_name_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").Named("default");

            TheUris[0].Name.LegacyShouldBe("default");
        }

        [Test]
        public void can_register_multiple_uris_for_a_resource()
        {
            ResourceSpaceHas.ResourcesOfType<Frodo>()
                .AtUri("/theshire")
                .And
                .AtUri("/lothlorien");

            TheUris.Count.LegacyShouldBe(2);
            TheUris[0].Uri.LegacyShouldBe("/theshire");
            TheUris[1].Uri.LegacyShouldBe("/lothlorien");
        }


        [Test]
        public void lcannot_register_a_null_uri_for_a_resource()
        {
            Executing(() => ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(null))
                .LegacyShouldThrow<ArgumentNullException>();
        }
    }

    public class when_registring_handlers_for_resources_with_URIs : configuration_context
    {
        [Test]
        public void a_handler_from_generic_type_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine")
                .HandledBy<CustomerHandler>();

            FirstRegistration.Handlers[0].Type.Name.LegacyShouldBe("CustomerHandler");
        }

        [Test]
        public void a_handler_from_itype_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType(typeof(Frodo)).AtUri("/theshrine")
                .HandledBy(TypeSystems.Default.FromClr(typeof(CustomerHandler)));
            FirstRegistration.Handlers[0].Type.Name.LegacyShouldBe("CustomerHandler");
        }

        [Test]
        public void a_handler_from_type_instance_is_registered()
        {
            ResourceSpaceHas.ResourcesOfType(typeof(Frodo)).AtUri("/theshrine")
                .HandledBy(typeof(CustomerHandler));
            FirstRegistration.Handlers[0].Type.Name.LegacyShouldBe("CustomerHandler");
        }

        [Test]
        public void cannot_add_a_null_handler()
        {
            Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy((IType)null))
                .LegacyShouldThrow<ArgumentNullException>();
            Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy((Type)null))
                .LegacyShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void two_handlers_can_be_added()
        {
            ResourceSpaceHas.ResourcesOfType(typeof(Frodo)).AtUri("/theshrine")
                .HandledBy<CustomerHandler>()
                .And
                .HandledBy<object>();

            FirstRegistration.Handlers[0].Type.Name.LegacyShouldBe("CustomerHandler");
            FirstRegistration.Handlers[1].Type.Name.LegacyShouldBe("Object");
        }
    }
    public class when_registering_uri_decorators : configuration_context
    {
        [Test]
        public void a_dependency_is_added_to_the_meta_model()
        {
            ResourceSpaceUses.UriDecorator<TestUriDecorator>();

            MetaModel.CustomRegistrations.OfType<DependencyRegistrationModel>().FirstOrDefault()
                .legacyShouldNotBeNull()
                .ConcreteType.LegacyShouldBe<TestUriDecorator>();
        }
        public class TestUriDecorator : IUriDecorator
        {
            public bool Parse(Uri uri, out Uri processedUri)
            {
                processedUri = null; return false;
            }

            public void Apply()
            {
            }
        }
    }


    public class when_registering_codecs : configuration_context
    {
        [Test]
        public void can_add_a_codec_by_type()
        {
            ExecuteTest(parent =>
                {
                    parent.TranscodedBy<CustomerCodec>();

                    FirstRegistration.Codecs[0].CodecType.LegacyShouldBe<CustomerCodec>();
                });
        }

        [Test]
        public void can_add_a_codec_by_type_instance()
        {
            ExecuteTest(parent =>
                {
                    parent.TranscodedBy(typeof(CustomerCodec));

                    FirstRegistration.Codecs[0].CodecType.LegacyShouldBe<CustomerCodec>();
                });
        }
        [Test]
        public void can_add_a_codec_configuration()
        {
            ExecuteTest(parent =>
            {
                var configurationObject = new object();
                parent.TranscodedBy(typeof(CustomerCodec),configurationObject);

                FirstRegistration.Codecs[0].Configuration.LegacyShouldBe(configurationObject);
            });
        }
        [Test]
        public void can_add_a_specific_media_type_for_a_codec()
        {
            ExecuteTest(parent =>
                {
                    parent.TranscodedBy<CustomerCodec>().ForMediaType(MediaType.Javascript.ToString());

                    FirstRegistration.Codecs[0].MediaTypes[0].MediaType.LegacyShouldBe(MediaType.Javascript);
                });
        }

        [Test]
        public void can_register_two_media_types()
        {
            ExecuteTest(parent =>
                {
                    parent.TranscodedBy<CustomerCodec>().ForMediaType("application/xhtml+xml").ForMediaType("text/plain");

                    FirstRegistration.Codecs[0].MediaTypes[0].MediaType.ToString().LegacyShouldBe("application/xhtml+xml");
                    FirstRegistration.Codecs[0].MediaTypes[1].MediaType.ToString().LegacyShouldBe("text/plain");
                });
        }
        [Test]
        public void can_register_an_extension_on_mediatype()
        {
            ExecuteTest(parent =>
            {
                parent.TranscodedBy<CustomerCodec>().ForMediaType("application/xhtml+xml").ForExtension("xml");

                FirstRegistration.Codecs[0].MediaTypes[0]
                    .Extensions.LegacyShouldContain("xml")
                    .Count().LegacyShouldBe(1);
            });
        }

        [Test]
        public void can_register_multiple_extensions_on_multiple_mediatypes()
        {
            ExecuteTest(parent =>
            {
                parent.TranscodedBy<CustomerCodec>()
                    .ForMediaType(MediaType.Xhtml).ForExtension("xml").ForExtension("xhtml")
                    .ForMediaType("text/html").ForExtension("html");

                FirstRegistration.Codecs[0].MediaTypes[0]
                    .Extensions
                        .LegacyShouldContain("xml")
                        .LegacyShouldContain("xhtml")
                        .Count().LegacyShouldBe(2);
                FirstRegistration.Codecs[0].MediaTypes[1]
                    .Extensions
                        .LegacyShouldContain("html")
                        .Count().LegacyShouldBe(1);
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
                    .ForMediaType("application/xhtml+xml").ForExtension("xml").ForExtension("xhtml")
                    .And
                    .TranscodedBy<CustomerWriterCodec>()
                    .ForMediaType("application/unknown");

                FirstRegistration.Codecs[0].CodecType.LegacyShouldBe<CustomerReaderCodec>();

                FirstRegistration.Codecs[1].CodecType.LegacyShouldBe<CustomerCodec>();
                FirstRegistration.Codecs[1].MediaTypes[0]
                    .Extensions
                        .LegacyShouldContain("xml")
                        .LegacyShouldContain("xhtml")
                        .Count().LegacyShouldBe(2);

                FirstRegistration.Codecs[2].CodecType.LegacyShouldBe<CustomerWriterCodec>();
                FirstRegistration.Codecs[2].MediaTypes[0]
                    .Extensions
                        .Count().LegacyShouldBe(0);
            });
        }
        [Test]
        public void cannot_register_codec_not_implementing_icodec()
        {
            Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().WithoutUri.TranscodedBy(typeof(string)))
                .LegacyShouldThrow<ArgumentOutOfRangeException>();
        }
        void ExecuteTest(Action<ICodecParentDefinition> test)
        {
            test(ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy<CustomerHandler>());
            MetaModel.ResourceRegistrations.Clear();
            test(ResourceSpaceHas.ResourcesOfType<Frodo>().WithoutUri);
        }
    }
    public class when_registering_custom_dependency : configuration_context
    {
        [Test]
        public void a_dependency_registration_is_added_to_the_metamodel()
        {
            ResourceSpaceUses.CustomDependency<IUriResolver, TemplatedUriResolver>(DependencyLifetime.Singleton);

            var first = MetaModel.CustomRegistrations.OfType<DependencyRegistrationModel>().LegacyShouldHaveCountOf(1).First();
            first.ConcreteType.LegacyShouldBe<TemplatedUriResolver>();
            first.ServiceType.LegacyShouldBe<IUriResolver>();
            first.Lifetime.LegacyShouldBe(DependencyLifetime.Singleton);

        }
    }
}