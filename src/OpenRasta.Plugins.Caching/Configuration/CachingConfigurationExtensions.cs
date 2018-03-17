using OpenRasta.Configuration.Fluent;

namespace OpenRasta.Plugins.Caching.Configuration
{
  public static class CachingConfigurationExtensions
  {
    public static T Caching<T>(this T uses) where T : IUses
    {
      return uses;
    }

    public static IResourceMapper<T> Map<T>(this IResource<T> resource)
    {
      return new ResourceMapper<T>(resource);
    }

    public static IResourceMapper<T> Map<T>(this IResource resource)
    {
      return new ResourceMapper<T>(resource);
    }
  }
}