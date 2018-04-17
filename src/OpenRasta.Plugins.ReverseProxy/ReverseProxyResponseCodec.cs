using System;
using System.Collections.Generic;
using System.Linq;
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
          foreach (var header in proxyResponse.ResponseMessage.Headers.Concat(proxyResponse.ResponseMessage.Content
            .Headers))
            SetHeader(response, header);

          response.Headers["via"] = string.Join(response.Headers["via"], $"1.1 {proxyResponse.Via}");
          await proxyResponse.ResponseMessage.Content.CopyToAsync(response.Stream);
        }
      }
      finally
      {
        proxyResponse.Dispose();
      }
    }

    static void SetHeader(IHttpEntity response, KeyValuePair<string, IEnumerable<string>> header)
    {
      var values = string.Join(", ", header.Value);

      if (AppendHeader(header.Key))
      {
        if (response.Headers.ContainsKey(header.Key))
          values = string.Join(",", response.Headers[header.Key], values);
      }

      response.Headers[header.Key] = values;
    }

    static bool AppendHeader(string name)
    {
      return string.Equals(name, "server-timing", StringComparison.OrdinalIgnoreCase);
    }
  }
}