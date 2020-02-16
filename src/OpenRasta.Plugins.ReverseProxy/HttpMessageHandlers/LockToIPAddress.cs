using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpMessageHandlers
{
  public class LockToIPAddress : DelegatingHandler
  {
    readonly Func<string, Task<IPAddress>> _dnsResolver;
    readonly SemaphoreSlim _dnsResolverLock = new SemaphoreSlim(1);
    public IPAddress Address { get; private set; }
    public string Host { get; private set; }

    public LockToIPAddress(
      HttpMessageHandler inner,
      Func<string, Task<IPAddress>> dnsResolver) : base(inner)
    {
      _dnsResolver = dnsResolver;
    }

    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var host = request.RequestUri.Host;
      await EnsureLockOnIPAddress(request, cancellationToken, host);

      if (string.Equals(host, Host, StringComparison.OrdinalIgnoreCase))
      {
        request.RequestUri = new UriBuilder(request.RequestUri) {Host = Address.ToString()}.Uri;
        request.Headers.Host = host;
      }
      
      return await base.SendAsync(request, cancellationToken);
    }

    async Task EnsureLockOnIPAddress(HttpRequestMessage request, CancellationToken cancellationToken, string host)
    {
      if (Address == null)
      {
        await _dnsResolverLock.WaitAsync(cancellationToken);
        try
        {
          if (Address == null)
          {
            Host = request.RequestUri.Host;
            if (IPAddress.TryParse(host, out var ip))
              Address = ip;
            else
              Address = await _dnsResolver(host);
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