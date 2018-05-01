using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpHandlers
{
  public class LockToIPAddress : DelegatingHandler
  {
    readonly Func<string, Task<IPAddress>> _dnsResolver;
    readonly SemaphoreSlim _dnsResolverLock = new SemaphoreSlim(1);
    IPAddress _ipAddress;
    string _rewrittenHost;

    public LockToIPAddress(HttpMessageHandler inner, Func<string,Task<IPAddress>> dnsResolver) : base(inner)
    {
      _dnsResolver = dnsResolver;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var host = request.RequestUri.Host;
      await EnsureLockOnIPAddress(request, cancellationToken, host);

      if (string.Equals(host, _rewrittenHost, StringComparison.OrdinalIgnoreCase))
      {
        request.RequestUri = new UriBuilder(request.RequestUri) {Host = _ipAddress.ToString()}.Uri;
        request.Headers.Host = host;
      }
      
      return await base.SendAsync(request, cancellationToken);
    }

    async Task EnsureLockOnIPAddress(HttpRequestMessage request, CancellationToken cancellationToken, string host)
    {
      if (_ipAddress == null)
      {
        await _dnsResolverLock.WaitAsync(cancellationToken);
        try
        {
          if (_ipAddress == null)
          {
            _rewrittenHost = request.RequestUri.Host;
            if (IPAddress.TryParse(host, out var ip))
              _ipAddress = ip;
            else
              _ipAddress = await _dnsResolver(host);
          }
        }
        finally
        {
          _dnsResolverLock.Release();
        }
      }
    }
  }
}