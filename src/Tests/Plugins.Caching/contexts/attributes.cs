using System;
using OpenRasta.Plugins.Caching;
using Tests.Plugins.Caching.attributes.proxy;

namespace Tests.Plugins.Caching.contexts
{
  public class attributes
  {
    CacheProxyAttribute _proxy;
    protected Exception exception;
    CacheServerAttribute _server;
    CacheBrowserAttribute _browser;
    protected TempObject cache;

    protected void given_attribute(
      CacheBrowserAttribute browser = null,
      CacheProxyAttribute proxy = null,
      CacheServerAttribute server = null)
    {
      _browser = browser;
      _proxy = proxy;
      _server = server;
    }

    protected void when_getting_response_caching()
    {
      throw new NotImplementedException();
    }
  }
}