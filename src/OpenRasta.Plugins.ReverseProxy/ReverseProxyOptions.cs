using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {
    public Func<HttpMessageHandler> HttpMessageHandler { get; set; } = () => new HttpClientHandler();
    public ForwardedHeaders FrowardedHeaders { get; set; } = new ForwardedHeaders();

    public class ForwardedHeaders
    {
      public bool ConvertLegacyHeaders { get; set; }
    }
  }
}