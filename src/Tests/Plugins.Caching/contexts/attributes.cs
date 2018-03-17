using System;
using OpenRasta.Plugins.Caching;
using OpenRasta.Plugins.Caching.Providers;

namespace Tests.Plugins.Caching.contexts
{
  public class attributes
  {
    CacheBrowserAttribute _browser;
    CacheProxyAttribute _proxy;
    protected ResponseCachingState cache;
    protected Exception exception;

    protected void given_attribute(
      CacheBrowserAttribute browser = null,
      CacheProxyAttribute proxy = null)
    {
      _browser = browser;
      _proxy = proxy;
    }

    protected void when_getting_response_caching()
    {
      try
      {
        cache = CacheResponse.GetResponseDirective(_proxy, _browser);
      }
      catch (Exception e)
      {
        exception = e;
      }
    }
  }
}