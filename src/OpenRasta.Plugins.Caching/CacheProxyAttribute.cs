namespace OpenRasta.Plugins.Caching
{
  public class CacheProxyAttribute : AbstractCacheAttribute
  {
    public CacheProxyAttribute()
    {
      Level = ProxyCacheLevel.Default;
    }

    public ProxyCacheLevel Level { get; set; }
  }

  public enum ProxyCacheLevel
  {
    Default,
    Everything,
    None
  }
}