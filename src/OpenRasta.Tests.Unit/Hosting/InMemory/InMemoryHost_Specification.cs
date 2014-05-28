using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Testing;

namespace OpenRasta.Tests.Unit.Hosting.InMemory
{
    public class when_creating_a_new_InMemoryHost : context
    {
        InMemoryHost _host;

        protected override void SetUp()
        {
            _host = new InMemoryHost(null);
        }

        [Test]
        public void the_resolver_is_an_internal_dependency_resolver()
        {
            _host.Resolver.ShouldNotBeNull();
            _host.Resolver.ShouldBeOfType<InternalDependencyResolver>();
        }
    }

    public class when_creating_a_new_InMemoryHost_with_custom_resolver : context
    {
        InMemoryHost _host;
        CustomResolver _customResolver;

        protected override void SetUp()
        {
            _customResolver = new CustomResolver();
            _host = new InMemoryHost(null, _customResolver);
        }

        [Test]
        public void the_resolver_is_a_custom_dependency_resolver()
        {
            _host.Resolver.ShouldBe(_customResolver);
        }

        class CustomResolver : InternalDependencyResolver {}
    }
}