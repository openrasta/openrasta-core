using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpMessageHandlers
{
  public class DelegateHandler : HttpMessageHandler
  {
    readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
      _sendAsync = sendAsync;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken) => _sendAsync(request, cancellationToken);
  }
}