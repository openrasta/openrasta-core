using System.Collections;
using System.Collections.Generic;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;

namespace Tests.Configuration.fluent_extensions
{
    public static class ResourceExtension
    {
        public static IResource Extended(this IResource resource, string name)
        {
            ((IResourceTarget)resource).Resource.Properties[name] = name;
            return resource;
        }
        public static IResource<T> Extended<T>(this IResource<T> resource, string name)
        {
            ((IResourceTarget)resource).Resource.Properties[name] = string.Format("{0}<{1}>", name, typeof(T).Name);
            return resource;
        }
    }
}