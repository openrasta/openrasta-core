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
    string _protocol;
    public object Configuration { get; set; }


    public ReverseProxyResponseCodec(ICommunicationContext context)
    {
      _response = context.Response;
      _protocol = context.PipelineData.Owin.RequestProtocol;
    }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      var proxyResponse = (ReverseProxyResponse) entity;
      try
      {
        _response.StatusCode = proxyResponse.StatusCode;

        if (proxyResponse.Via != null)
          response.Headers.Add("via", $"{_protocol} {proxyResponse.Via}");

        if (proxyResponse.ResponseMessage != null)
        {
          foreach (var header in
            proxyResponse.ResponseMessage.Headers.Concat(
              proxyResponse.ResponseMessage.Content.Headers))
          {
            SetHeader(response, header);
          }

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
        response.Headers.Add(header.Key, values);
      }
      else
      {
        response.Headers[header.Key] = values;
      }
    }
  }
}