using System;

namespace OpenRasta.Plugins.Caching
{
  public abstract class AbstractCacheAttribute : Attribute
  {
    public bool? Store { get; set; }
    public bool MustRevalidate { get; set; }
    public string MaxAge { get; set; }
  }
}