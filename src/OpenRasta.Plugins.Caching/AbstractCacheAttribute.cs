using System;

namespace OpenRasta.Plugins.Caching
{
  public abstract class AbstractCacheAttribute : Attribute
  {
    public bool? Store { get; set; }
    public bool MustRevalidateWhenStale { get; set; }
    public string MaxAge { get; set; }
  }
}