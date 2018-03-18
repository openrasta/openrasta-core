using System;

namespace OpenRasta.Plugins.Caching
{
  public class CacheClientAttribute : AbstractCacheAttribute
  {
    public CacheLevel Level { get; set; }
  }
}