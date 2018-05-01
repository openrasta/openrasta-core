using System;
using System.Net.Http;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {
    public ReverseProxyOptions()
    {
    }
    public ViaOptions Via { get; } = new ViaOptions();
    public ForwardedHeaders FrowardedHeaders { get; } = new ForwardedHeaders();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public HttpClientOptions HttpClient { get; set; } = new HttpClientOptions();

    public class ViaOptions
    {
      public string Pseudonym { get; set; }
    }
    public class ForwardedHeaders
    {
      public bool ConvertLegacyHeaders { get; set; }
      public bool RunAsForwardedHost { get; set; }
    }

    public class HttpClientOptions
    {
      Func<HttpClient> _factory;
      public Func<HttpMessageHandler> Handler { get; set; } = () => new HttpClientHandler();

      public Func<HttpClient> Factory
      {
        get => _factory ?? (()=>new HttpClient(Handler()));
        set => _factory = value;
      }
    }
  }
}