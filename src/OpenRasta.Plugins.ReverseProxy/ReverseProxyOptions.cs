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
      Func<string,HttpClient> _factory;
      public Func<HttpMessageHandler> Handler { get; set; } = () => new HttpClientHandler()
      {
        AllowAutoRedirect = false
      };

      public Func<string, HttpClient> Factory
      {
        get => _factory ?? ((domain)=>new HttpClient(Handler()));
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
      public Func<Exception,string,HostEvictionAction> OnHostEvicted { get; set; } = (e,h)=>HostEvictionAction.Evict;
      public Action<Exception> OnError { get; set; } = (e)=>{};
      
    }
  }

  public enum HostEvictionAction
  {
    None,
    Evict
  }
}