using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {
    public Func<HttpMessageHandler> HttpMessageHandler { get; set; } = () => new HttpClientHandler();
  }
}