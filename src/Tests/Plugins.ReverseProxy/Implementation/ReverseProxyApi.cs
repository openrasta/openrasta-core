using System;
using OpenRasta.Configuration;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ReverseProxyApi : IConfigurationSource
  {
    readonly string from;
    readonly string to;
    readonly ReverseProxyOptions options;
    readonly Func<ICommunicationContext, string> operation;

    public ReverseProxyApi(
        string from,
        string to,
        ReverseProxyOptions options,
        Func<ICommunicationContext,string> operation = null)
    {
      this.from = from;
      this.to = to;
      this.options = options;
      this.operation = operation ?? (ctx=>ctx.Request.Uri.ToString());
    }

    public void Configure()
    {
      ResourceSpace.Has
          .ResourcesNamed("to")
          .AtUri(to)
          .HandledBy<ProxiedHandler>()
          .TranscodedBy<ProxiedCodec>()
          .ForMediaType("text/plain");

      ResourceSpace.Has
          .ResourcesNamed("from")
          .AtUri(from)
          .ReverseProxyFor($"http://localhost{to}");

      ResourceSpace.Uses.Dependency(d => d.Transient((ICommunicationContext context) => new ProxiedHandler(context, operation)));
      
      ResourceSpace.Uses.ReverseProxy(options);
    }
  }
}