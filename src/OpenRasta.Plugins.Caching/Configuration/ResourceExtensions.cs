using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Pipeline;
using OpenRasta.TypeSystem;

namespace OpenRasta.Plugins.Caching.Configuration
{
  public static class ResourceExtensions
  {
    public static Func<object, DateTimeOffset?> GetLastModifiedMapper(this ResourceModel resource)
    {
      return resource.Properties.ContainsKey(CacheKeys.MappersLastModified)
        ? (Func<object, DateTimeOffset?>) resource.Properties[CacheKeys.MappersLastModified]
        : obj => null;
    }

    public static void SetLastModifiedMapper(this ResourceModel resource, Func<object, DateTimeOffset?> mapper)
    {
      resource.Properties[CacheKeys.MappersLastModified] = mapper;
    }

    public static Func<object, string> GetEtagMapper(this ResourceModel resource)
    {
      return resource.Properties.ContainsKey(CacheKeys.MappersEtag)
        ? (Func<object, string>) resource.Properties[CacheKeys.MappersEtag]
        : obj => null;
    }

    public static void SetEtagMapper(this ResourceModel resource, Func<object, string> mapper)
    {
      resource.Properties[CacheKeys.MappersEtag] = mapper;
    }

    public static Func<object, TimeSpan?> GetExpires(this ResourceModel resource)
    {
      return resource.Properties.ContainsKey(CacheKeys.MappersExpires)
        ? (Func<object, TimeSpan?>) resource.Properties[CacheKeys.MappersExpires]
        : obj => null;
    }

    public static void SetExpires(this ResourceModel resource, Func<object, TimeSpan?> mapper)
    {
      resource.Properties[CacheKeys.MappersExpires] = mapper;
    }

    public static DateTimeOffset GetCachingTime(this PipelineData data)
    {
      DateTimeOffset now;
      if (data.ContainsKey(CacheKeys.Now)) return (DateTimeOffset) data[CacheKeys.Now];
      data[CacheKeys.Now] = now = ServerClock.UtcNow();
      return now;
    }

    public static IEnumerable<ResourceModel> FindAll(this IEnumerable<ResourceModel> resources, Type resourceType)
    {
      if (resources == null) throw new ArgumentNullException(nameof(resources));
      if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

      return from resource in resources
        let typedRegistrationKey = resource.ResourceKey as IType
        where typedRegistrationKey != null
        let typeSystem = typedRegistrationKey.TypeSystem
        let distance = typeSystem.FromClr(resourceType).CompareTo(typedRegistrationKey)
        where distance >= 0
        orderby distance
        select resource;
    }
  }
}