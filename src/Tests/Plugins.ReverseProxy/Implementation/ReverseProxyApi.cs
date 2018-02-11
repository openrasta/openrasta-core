using System;
using OpenRasta.Configuration;
using OpenRasta.Plugins.ReverseProxy;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ReverseProxyApi : IConfigurationSource
  {
    readonly ReverseProxyOptions _options;

    public ReverseProxyApi(ReverseProxyOptions options)
    {
      _options = options;
    }

    public void Configure()
    {
      ResourceSpace.Has
          .ResourcesNamed("proxied")
          .AtUri("/proxied")
          .And.AtUri("/proxied-with-qs?q={query}")
          .HandledBy<ProxiedHandler>()
          .TranscodedBy<ProxiedCodec>()
          .ForMediaType("text/plain");
      
      ResourceSpace.Has
        .ResourcesNamed("proxy")
        .AtUri("/proxy")
        .ReverseProxyFor("http://localhost/proxied");
      
      ResourceSpace.Has
          .ResourcesNamed("proxy-wth-qs")
          .AtUri("/proxy-with-qs")
          .ReverseProxyFor("http://localhost/proxied-with-qs");

      ResourceSpace.Uses.ReverseProxy(_options);
    }
  }
}