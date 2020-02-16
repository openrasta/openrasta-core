using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
          SetHeaders(response.Headers, proxyResponse.ResponseMessage.Headers);
          SetHeaders(response.Headers, proxyResponse.ResponseMessage.Content.Headers);

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

    void SetHeaders(HttpHeaderDictionary headers, HttpContentHeaders responseMessageHeaders)
    {
      foreach (var header in responseMessageHeaders)
        SetHeader(headers, header.Key, header.Value);
    }

    void SetHeaders(HttpHeaderDictionary headers, HttpResponseHeaders responseMessageHeaders)
    {
      foreach (var header in responseMessageHeaders)
        SetHeader(headers, header.Key, header.Value);
    }

    static void SetHeader(HttpHeaderDictionary headers, string fieldName, IEnumerable<string> fieldValues)
    {
      if (HttpHeaderClassification.IsHopByHopHeader(fieldName)) return;

      if (HttpHeaderClassification.IsAppendedOnForwardHeader(fieldName))
      {
        headers.AddValues(fieldName, fieldValues);
      }
      else
      {
        headers.Remove(fieldName);
        headers.AddValues(fieldName, fieldValues);
      }
    }
  }
}