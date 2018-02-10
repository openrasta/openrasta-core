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
    public object Configuration { get; set; }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var proxyResponse = (HttpResponseMessage) entity;
      foreach (var header in proxyResponse.Headers)
        response.Headers[header.Key] = string.Join(", ", header.Value);
      await proxyResponse.Content.CopyToAsync(response.Stream);
    }
  }
}