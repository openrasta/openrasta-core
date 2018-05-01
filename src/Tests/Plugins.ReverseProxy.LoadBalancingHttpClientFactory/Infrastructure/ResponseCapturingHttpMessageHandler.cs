using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Plugins.ReverseProxy.LoadBalancingHttpClientFactory.Infrastructure
{
  class ResponseCapturingHttpMessageHandler : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      Request = request;
      return Task.FromResult(new HttpResponseMessage());
    }

    public HttpRequestMessage Request { get; private set; }
  }
}