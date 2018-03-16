using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyResponseCodec : IMediaTypeWriterAsync
  {
    readonly IResponse _response;
    public object Configuration { get; set; }

    public ReverseProxyResponseCodec(IResponse response)
    {
      _response = response;
    }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var proxyResponse = (ReverseProxyResponse) entity;
      try
      {
        _response.StatusCode = proxyResponse.StatusCode;

        if (proxyResponse.ResponseMessage != null)
        {
          foreach (var header in proxyResponse.ResponseMessage.Headers)
            response.Headers[header.Key] = string.Join(", ", header.Value);

          await proxyResponse.ResponseMessage.Content.CopyToAsync(response.Stream);
        }
      }
      finally
      {
        proxyResponse.Dispose();
      }
    }
  }
}