using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyResponseCodec : IMediaTypeWriterAsync
  {
    readonly IResponse _response;
    readonly ICommunicationContext _context;
    public object Configuration { get; set; }


    public ReverseProxyResponseCodec(ICommunicationContext context)
    {
      _response = context.Response;
      _context = context;
    }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var proxyResponse = (ReverseProxyResponse) entity;
      try
      {
        _response.StatusCode = proxyResponse.StatusCode;

        response.Headers["via"] = string.Join(response.Headers["via"], $"1.1 {proxyResponse.Via}");

        if (proxyResponse.ResponseMessage != null)
        {
          foreach (var header in proxyResponse.ResponseMessage.Headers.Concat(proxyResponse.ResponseMessage.Content
            .Headers))
            SetHeader(response, header);

          await proxyResponse.ResponseMessage.Content.CopyToAsync(response.Stream);
        }
        else if (proxyResponse.Error != null)
        {
          var errorMessage = Encoding.UTF8.GetBytes(proxyResponse.Error.ToString());
          await response.Stream.WriteAsync(errorMessage, 0, errorMessage.Length);
        }
      }
      finally
      {
        proxyResponse.Dispose();
      }
    }

    static void SetHeader(IHttpEntity response, KeyValuePair<string, IEnumerable<string>> header)
    {
      if (HttpHeaderClassification.IsHopByHopHeader(header.Key)) return;
      
      var values = string.Join(", ", header.Value);

      if (HttpHeaderClassification.IsAppendedOnForwardHeader(header.Key))
      {
        if (response.Headers.ContainsKey(header.Key))
          values = string.Join(",", response.Headers[header.Key], values);
      }

      response.Headers[header.Key] = values;
    }
  }
}