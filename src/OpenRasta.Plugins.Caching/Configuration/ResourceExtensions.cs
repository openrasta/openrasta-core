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
      return resource.Properties.ContainsKey(Keys.MAPPERS_LAST_MODIFIED)
        ? (Func<object, DateTimeOffset?>) resource.Properties[Keys.MAPPERS_LAST_MODIFIED]
        : obj => null;
    }

    public static void SetLastModifiedMapper(this ResourceModel resource, Func<object, DateTimeOffset?> mapper)
    {
      resource.Properties[Keys.MAPPERS_LAST_MODIFIED] = mapper;
    }

    public static Func<object, string> GetEtagMapper(this ResourceModel resource)
    {
      return resource.Properties.ContainsKey(Keys.MAPPERS_ETAG)
        ? (Func<object, string>) resource.Properties[Keys.MAPPERS_ETAG]
        : obj => null;
    }

    public static void SetEtagMapper(this ResourceModel resource, Func<object, string> mapper)
    {
      resource.Properties[Keys.MAPPERS_ETAG] = mapper;
    }

    public static Func<object, TimeSpan?> GetExpires(this ResourceModel resource)
    {
      return resource.Properties.ContainsKey(Keys.MAPPERS_EXPIRES)
        ? (Func<object, TimeSpan?>) resource.Properties[Keys.MAPPERS_EXPIRES]
        : obj => null;
    }

    public static void SetExpires(this ResourceModel resource, Func<object, TimeSpan?> mapper)
    {
      resource.Properties[Keys.MAPPERS_EXPIRES] = mapper;
    }

    public static DateTimeOffset GetCachingTime(this PipelineData data)
    {
      DateTimeOffset now;
      if (data.ContainsKey(Keys.NOW)) return (DateTimeOffset) data[Keys.NOW];
      data[Keys.NOW] = now = ServerClock.UtcNow();
      return now;
    }

    public static ResourceModel Find(this IEnumerable<ResourceModel> resources, object resourceKey)
    {
      if (resources == null) throw new ArgumentNullException(nameof(resources));

      return resources.FirstOrDefault(_ => _.ResourceKey == resourceKey)
             ?? FindBestMatchByType(resources, resourceKey).FirstOrDefault();
    }

    public static IEnumerable<ResourceModel> FindAll(this IEnumerable<ResourceModel> resources, object resourceKey)
    {
      if (resources == null) throw new ArgumentNullException(nameof(resources));

      var exactmatch = resources.FirstOrDefault(_ => _.ResourceKey == resourceKey);
      if (exactmatch != null) yield return exactmatch;
      foreach (var res in FindBestMatchByType(resources, resourceKey)) yield return res;
    }

    static IEnumerable<ResourceModel> FindBestMatchByType(IEnumerable<ResourceModel> resources, object resourceKey)
    {
      var typedResourceKey = resourceKey as Type;
      if (typedResourceKey == null) return null;
      return from resource in resources
        let typedRegistrationKey = resource.ResourceKey as IType
        let typeSystem = typedRegistrationKey.TypeSystem
        where typedRegistrationKey != null
        let distance = typeSystem.FromClr(typedResourceKey).CompareTo(typedRegistrationKey)
        where distance >= 0
        orderby distance
        select resource;
    }
  }
}