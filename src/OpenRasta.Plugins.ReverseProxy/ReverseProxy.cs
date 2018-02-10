using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Web;
using HttpMethod = System.Net.Http.HttpMethod;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxy
  {
    readonly IMetaModelRepository _modelRepository;
    Lazy<HttpClient> _httpClient;

    public ReverseProxy(ReverseProxyOptions options, IMetaModelRepository modelRepository)
    {
      _modelRepository = modelRepository;
      _httpClient = new Lazy<HttpClient>(
          () => new HttpClient(options.HttpMessageHandler()),
          LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<HttpResponseMessage> Send(ICommunicationContext context, Uri target)
    {
      var request = new HttpRequestMessage()
      {
          RequestUri = target,
          Method = new HttpMethod(context.Request.HttpMethod),
          Content = { }
      };
      foreach (var header in context.Request.Headers)
        request.Headers.Add(header.Key, header.Value);

      return await _httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    }
  }
}