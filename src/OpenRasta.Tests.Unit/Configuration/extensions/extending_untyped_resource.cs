using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Testing;

namespace OpenRasta.Tests.Unit.Configuration.extensions
{
    public class extending_untyped_resource : contexts.configuration
    {
        public extending_untyped_resource()
        {
            given_has(_ => _.ResourcesNamed("Anytyhing").Extended("test"));
            when_configuration_completed();

        }
        [Test]
        public void resource_is_configured()
        {
            Configuration.ResourceRegistrations.SingleOrDefault()
                    .ShouldNotBeNull();
        }
        [Test]
        public void property_is_persisted()
        {
            Configuration.ResourceRegistrations.Single()
                    .Properties["test"].ShouldBe("test");

        }
    }
    public class extending_typed_resource : contexts.configuration
    {
        public extending_typed_resource()
        {
            given_has(_ => _.ResourcesOfType<Resource>().ExtendedTypedResource());
            when_configuration_completed();

        }
        [Test]
        public void resource_is_configured()
        {
            Configuration.ResourceRegistrations.SingleOrDefault()
                    .ShouldNotBeNull();
        }
        [Test]
        public void property_is_persisted()
        {
            Configuration.ResourceRegistrations.Single()
                    .Properties["test"].ShouldBe("test");

        }
    }

    public static class ResourceExtension
    {
        public static T Extended<T>(this T resource, string name) where T:IResource
        {
            ((IResourceTarget)resource).Resource.Properties[name] = name;
            return resource;
        }

    }
    public static class DemoExtensions
    {
        public static IResource<TResource> ExtendedTypedResource<TResource>(this IResource<TResource> resource)
        {
            ((IResourceTarget)resource).Resource.Properties["extended"] = true;
            return resource;
        }

        public static T ExtendedTypedUri<T, TResource>(this T resource) where T : IUri<TResource>
        {
            ((IUriTarget)resource).Uri.Properties["extended"] = true;
            return resource;
        }

        public static T ExtendedTypedHandler<T, TResource, THandler>(this T resource) where T : IHandler<TResource, THandler
    >
    {
        ((IHandlerTarget)resource).Handler.Properties["extended"] = true;
        return resource;
    }

        public static T ExtendedTypedCodec<T, TResource, TCodec>(this T resource) where T : ICodec<TResource, TCodec
    >
        {
            ((ICodecTarget)resource).Codec.Properties["extended"] = true;
            return resource;
        }

    }
}