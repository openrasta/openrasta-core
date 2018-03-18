namespace OpenRasta.Plugins.Caching
{
  public class CacheProxyAttribute : AbstractCacheAttribute
  {
    public CacheProxyAttribute()
    {
      Level = CacheLevel.Cacheable;
    }

    public CacheLevel Level { get; set; }
  }
}