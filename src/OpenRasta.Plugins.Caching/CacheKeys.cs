namespace OpenRasta.Plugins.Caching
{
  public static class CacheKeys
  {
    public const string ResponseCache = "openrasta.caching.response";
    public const string RewriteTo304 = "openrasta.caching.rewrite304";
    public const string LastModified = "openrasta.caching.lastmodified";
    public const string MappersLastModified = "openrasta.caching.mappers.last-modified";
    public const string MappersEtag = "openrasta.caching.mappers.etag";
    public const string MappersExpires = "openrasta.caching.mappers.expires";
    public const string Now = "openrasta.caching.now";
  }
}