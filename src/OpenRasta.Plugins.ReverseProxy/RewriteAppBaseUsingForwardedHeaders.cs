using System;
using System.Linq;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class RewriteAppBaseUsingForwardedHeaders : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(RewriteAppBase).After<KnownStages.IBegin>().And.Before<KnownStages.IAuthentication>();
    }

    PipelineContinuation RewriteAppBase(ICommunicationContext ctx)
    {
      if (!ctx.Request.Headers.TryGetValue("forwarded", out var headerValue))
        return PipelineContinuation.Continue;
      
      var header = ForwardedHeader.Parse(headerValue).FirstOrDefault();
      if (header == null) return PipelineContinuation.Continue;
      var appBaseUri = new UriBuilder(ctx.Request.Uri);

      if (header.TryGetValue("host", out var host))
        appBaseUri.Host = host;

      var hasPort = header.TryGetValue("port", out var sport);
      if (hasPort)
      {
        if (int.TryParse(sport, out var port)) appBaseUri.Port = port;
      }

      if (header.TryGetValue("proto", out var proto))
      {
        appBaseUri.Scheme = proto;
        if (!hasPort)
          appBaseUri.Port = -1;
      }

      if (header.TryGetValue("base", out var baseUri) && appBaseUri.Path.StartsWith(baseUri))
      {
        appBaseUri.Path = appBaseUri.Path.Substring(baseUri.Length);
      }

      ctx.Request.Uri = appBaseUri.Uri;
      
      return PipelineContinuation.Continue;
    }
  }
}