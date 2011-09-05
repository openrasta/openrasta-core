using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;

namespace OpenRasta.Tests.Unit.Configuration.extensions
{
    public class extending_resource : configuration_context
    {
        public extending_resource()
        {
            when_has(_ => _.ResourcesNamed("Anytyhing").Extended("test"));
        }
        [Test]
        public void property_is_persisted()
        {
            
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
}