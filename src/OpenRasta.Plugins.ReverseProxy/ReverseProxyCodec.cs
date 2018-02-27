using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyCodec : IMediaTypeWriterAsync
  {
    readonly IResponse _response;
    public object Configuration { get; set; }

    public ReverseProxyCodec(IResponse response)
    {
      _response = response;
    }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var proxyResponse = (HttpResponseMessage) entity;
      _response.StatusCode = (int)proxyResponse.StatusCode;
      foreach (var header in proxyResponse.Headers)
        response.Headers[header.Key] = string.Join(", ", header.Value);
      
      await proxyResponse.Content.CopyToAsync(response.Stream);
    }
  }
}