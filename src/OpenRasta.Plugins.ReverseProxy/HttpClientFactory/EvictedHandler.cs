using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy.HttpClientFactory
{
  class EvictedHandler
  {
    readonly WeakReference _weakReference;

    public EvictedHandler(ActiveHandler handler)
    {
      _disposableHandler = handler.InnerHandler;
      _weakReference = new WeakReference(handler);
    }

    readonly HttpMessageHandler _disposableHandler;
    public bool IsDisposable => !_weakReference.IsAlive;

    public void Dispose()
    {
      _disposableHandler.Dispose();
    }
  }
}