namespace OpenRasta.Plugins.Caching
{
  public class CacheBrowserAttribute : AbstractCacheAttribute
  {
    public BrowserCacheLevel Level { get; set; }
  }

  public enum BrowserCacheLevel
  {
    Default,
    None
  }
}