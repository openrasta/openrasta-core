JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿using System.Linq;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Testing;
using OpenRasta.TypeSystem;
using OpenRasta.Configuration.Fluent;

namespace Tests.Configuration.fluent_extensions
{
    public class untyped_resource : contexts.configuration
    {
        public untyped_resource()
        {
            given_has(_ => _.ResourcesNamed("Anytyhing").Extended("test"));
            when_configured();
        }
        [Test]
        public void resource_is_registered()
        {
            Config.ResourceRegistrations.ShouldHaveCountOf(1);
        }

        [Test]
        public void property_is_persisted()
        {
            Config.ResourceRegistrations.Single().Properties["test"].ShouldBe("test");
        }
    }

    public class untyped_resource_after_codec : contexts.configuration
    {
        public untyped_resource_after_codec()
        {
            given_has(_ => _.ResourcesNamed("Anytyhing").WithoutUri.AsJsonDataContract().Extended("test"));
            when_configured();
        }
        [Test]
        public void resource_is_registered()
        {
            Config.ResourceRegistrations.ShouldHaveCountOf(1);
        }

        [Test]
        public void property_is_persisted()
        {
            Config.ResourceRegistrations.Single().Properties["test"].ShouldBe("test");
        }
    }
    //public class untyped_resource_continuation_handler : contexts.configuration
    //{
    //    public untyped_resource_continuation_handler()
    //    {
    //        given_has(_ => _.ResourcesNamed("Anytyhing").Extended("test").HandledBy<Resource>());
    //        when_configured();
    //    }
    //    [Test]
    //    public void handler_is_persisted()
    //    {
    //        Config.ResourceRegistrations.Single().Handlers.SingleOrDefault(x => x.Type == TypeSystem.FromClr<Resource>()).ShouldNotBeNull();
    //    }
    //}
    public class typed_resource : contexts.configuration
    {
        public typed_resource()
        {
            given_has<Resource>(_ => _.Extended("test"));
            when_configured();
        }
        [Test]
        public void resource_is_registered()
        {
            Config.ResourceRegistrations.ShouldHaveCountOf(1);
        }

        [Test]
        public void property_is_persisted()
        {
            Config.ResourceRegistrations.Single().Properties["test"].ShouldBe("test<Resource>");
        }
    }
    public class typed_resource_continuation_uri : contexts.configuration
    {
        public typed_resource_continuation_uri()
        {
            given_has<Resource>(_ => _.Extended("test").AtUri("/uri"));
            when_configured();
        }
    }
    //public class typed_resource_continuation_handled : contexts.configuration
    //{
    //    public typed_resource_continuation_handled()
    //    {
    //        given_has<Resource>(_ => _.Extended("test").HandledBy<Resource>());
    //        when_configured();
    //    }
    //}
    class Resource{}
}