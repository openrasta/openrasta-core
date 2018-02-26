using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {
    public ViaOptions Via { get; } = new ViaOptions();
    public Func<HttpMessageHandler> HttpMessageHandler { get; set; } = () => new HttpClientHandler();
    public ForwardedHeaders FrowardedHeaders { get; } = new ForwardedHeaders();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public class ViaOptions
    {
      public string Pseudonym { get; set; }
    }
    public class ForwardedHeaders
    {
      public bool ConvertLegacyHeaders { get; set; }
      public bool RunAsForwardedHost { get; set; }
    }
  }
}