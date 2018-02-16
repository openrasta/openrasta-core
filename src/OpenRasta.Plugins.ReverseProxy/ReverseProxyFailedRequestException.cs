using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyFailedRequestException : Exception
  {
    [JsonProperty("Request")]
    public HttpRequestMessage Request { get; }

    public ReverseProxyFailedRequestException(HttpRequestMessage request, Exception innerException)
        : base($"A proxied request failed.{Environment.NewLine}{request}", innerException)
    {
      Request = request;
    }
  }
}