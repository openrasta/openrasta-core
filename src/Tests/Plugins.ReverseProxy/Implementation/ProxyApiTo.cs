using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxyApiTo : IConfigurationSource
  {
    readonly string to;
    readonly ReverseProxyOptions options;
    readonly Func<ICommunicationContext, Task<string>> operation;

    public ProxyApiTo(string to,
        ReverseProxyOptions options,
        Func<ICommunicationContext,Task<string>> operation = null)
    {
      this.to = to;
      this.options = options;
      this.operation = operation ?? (async ctx=>ctx.Request.Uri.ToString());
    }

    public void Configure()
    {
      ResourceSpace.Has
          .ResourcesNamed("to")
          .AtUri(to)
          .HandledBy<ProxiedHandler>()
          .TranscodedBy<ProxiedCodec>()
          .ForMediaType("text/plain");
      
      ResourceSpace.Uses.Dependency(d => d.Transient((ICommunicationContext context) => new ProxiedHandler(context, operation)));
      
      ResourceSpace.Uses.ReverseProxy(options);
    }
  }
}