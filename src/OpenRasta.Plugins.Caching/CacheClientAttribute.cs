using System;

namespace OpenRasta.Plugins.Caching
{
  public class CacheClientAttribute : AbstractCacheAttribute
  {
    public CacheClientAttribute()
    {
      Level = CacheLevel.Cacheable;
    }
    public CacheLevel Level { get; set; }
  }
}