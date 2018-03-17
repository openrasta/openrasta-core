namespace OpenRasta.Plugins.Caching
{
  public static class Keys
  {
    public const string RESPONSE_CACHE = "openrasta.caching.response";
    public const string REWRITE_TO_304 = "openrasta.caching.rewrite304";
    public const string LAST_MODIFIED = "openrasta.caching.lastmodified";
    public static string MAPPERS_LAST_MODIFIED = "openrasta.caching.mappers.last-modified";
    public static string MAPPERS_ETAG = "openrasta.caching.mappers.etag";
    public static string MAPPERS_EXPIRES = "openrasta.caching.mappers.expires";
    public static string NOW = "openrasta.caching.now";
  }
}