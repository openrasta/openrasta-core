using System;
using System.Collections.Generic;
using System.Net;
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
    readonly List<IPAddress> networkIpAddresses;

    public ProxyApiFrom(
      string from,
      string to,
      ReverseProxyOptions options,
      string localIpAddress = null,
      List<IPAddress> networkIpAddresses = null)
    {
      this.from = from;
      this.to = to;
      this.options = options;
      this.localIpAddress = localIpAddress;
      this.networkIpAddresses = networkIpAddresses;
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
      ResourceSpace.Uses.PipelineContributor(() => new SetNetworkIpAddresses(networkIpAddresses));
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
  
  public class SetNetworkIpAddresses : IPipelineContributor
  {
    readonly List<IPAddress> networkIpAddresses;
    
    public SetNetworkIpAddresses(List<IPAddress> networkIpAddresses)
    {
      this.networkIpAddresses = networkIpAddresses;
    }
    
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(context =>
        {
          if (networkIpAddresses != null)
          {
            context.PipelineData["network.ipAddresses"] = networkIpAddresses;
          }
          return PipelineContinuation.Continue;
        })
        .Before<KnownStages.IAuthentication>();
    }
  }
}