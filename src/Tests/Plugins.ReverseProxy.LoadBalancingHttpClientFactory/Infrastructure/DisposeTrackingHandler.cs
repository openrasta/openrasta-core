using System.Net.Http;

namespace Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory.Infrastructure
{
  class DisposeTrackingHandler : DelegatingHandler
  {
    public DisposeTrackingHandler(HttpMessageHandler inner) : base(inner)
    {
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      IsDisposed = true;
    }

    public bool IsDisposed { get; private set; }
  }
}