using System;
using OpenRasta.Plugins.Caching;
using OpenRasta.Plugins.Caching.Providers;

namespace Tests.Plugins.Caching.contexts
{
  public class attributes
  {
    CacheClientAttribute client;
    CacheProxyAttribute _proxy;
    protected ResponseCachingState cache;
    protected Exception exception;

    protected void given_attribute(
      CacheClientAttribute client = null,
      CacheProxyAttribute proxy = null)
    {
      this.client = client;
      _proxy = proxy;
    }

    protected void when_getting_response_caching()
    {
      try
      {
        cache = CacheResponse.GetResponseDirective(_proxy, client);
      }
      catch (Exception e)
      {
        exception = e;
      }
    }
  }
}