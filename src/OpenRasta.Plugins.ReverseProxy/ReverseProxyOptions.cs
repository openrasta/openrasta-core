using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxyOptions
  {

    public ReverseProxyOptions()
    {
    }
    public ViaOptions Via { get; } = new ViaOptions();
    public ForwardedHeadersOptions ForwardedHeaders { get; } = new ForwardedHeadersOptions();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public HttpClientOptions HttpClient { get; set; } = new HttpClientOptions();
    public Action<ICommunicationContext,HttpRequestMessage> OnSend { get; set; }
    public Action<ReverseProxyResponse> OnProxyResponse { get; set; }

    public class ViaOptions
    {
      public string Pseudonym { get; set; }
    }
    public class ForwardedHeadersOptions
    {
      public bool ConvertLegacyHeaders { get; set; }
      public bool RunAsForwardedHost { get; set; }
    }

    public class HttpClientOptions
    {
      Func<HttpClient> _factory;
      public Func<HttpMessageHandler> Handler { get; set; } = () => new HttpClientHandler()
      {
        AllowAutoRedirect = false
      };

      public Func<HttpClient> Factory
      {
        get => _factory ?? (()=>new HttpClient(Handler()));
        set => _factory = value;
      }

      public RoundRobinOptions RoundRobin { get; } = new RoundRobinOptions();
    }

    public class RoundRobinOptions
    {
      public bool Enabled { get; set; }
      public int ClientCount { get; set; }
      public TimeSpan LeaseTime { get; set; }
      public bool ClientPerNode { get; set; }
      public Func<string,Task<IPAddress[]>> DnsResolver { get; set; }
      public DnsResolverResponseType DnsResolverResponseType { get; set; } = DnsResolverResponseType.Partial;
      
    }

    public enum DnsResolverResponseType
    {
      Partial,
      All
    }
  }

}