using System;
using OpenRasta.Configuration;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.ReverseProxy;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ProxyApiFrom : IConfigurationSource
  {
    readonly string from;
    readonly string to;
    readonly ReverseProxyOptions options;
    readonly string localIpAddress;

    public ProxyApiFrom(
      string from,
      string to,
      ReverseProxyOptions options,
      string localIpAddress = null)
    {
      this.from = from;
      this.to = to;
      this.options = options;
      this.localIpAddress = localIpAddress;
    }

    public void Configure()
    {
      ResourceSpace.Has
        .ResourcesNamed("from")
        .AtUri(from)
        .ReverseProxyFor(new Uri(new Uri("http://destination.example", UriKind.Absolute), to).ToString());

      ResourceSpace.Uses.ReverseProxy(options);
      ResourceSpace.Uses.PipelineContributor<WriteServerTimingHeader>();
      
      ResourceSpace.Uses.PipelineContributor(() => new SetLocalIpAddress(localIpAddress));
    }
  }

  public class WriteServerTimingHeader : IPipelineContributor
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(context =>
        {
          context.Response.Headers["Server-Timing"] = "from;dur=1";
          return PipelineContinuation.Continue;
        })
        .Before<KnownStages.IResponseCoding>();
    }
  }

  public class SetLocalIpAddress : IPipelineContributor
  {
    readonly string localIpAddress;
    
    public SetLocalIpAddress(string localIpAddress)
    {
      this.localIpAddress = localIpAddress;
    }
    
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(context =>
        {
          if (localIpAddress != null)
          {
            context.PipelineData["server.localIpAddress"] = localIpAddress;
          }
          else
          {
            context.PipelineData.Remove("server.localIpAddress");
          }
          return PipelineContinuation.Continue;
        })
        .Before<KnownStages.IAuthentication>();
    }
  }
}