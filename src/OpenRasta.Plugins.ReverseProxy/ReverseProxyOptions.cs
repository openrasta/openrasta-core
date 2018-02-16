using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {
    public Func<HttpMessageHandler> HttpMessageHandler { get; set; } = () => new HttpClientHandler();
    public ForwardedHeaders FrowardedHeaders { get; set; } = new ForwardedHeaders();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public class ForwardedHeaders
    {
      public bool ConvertLegacyHeaders { get; set; }
      public bool RunAsForwardedHost { get; set; }
    }
  }
}