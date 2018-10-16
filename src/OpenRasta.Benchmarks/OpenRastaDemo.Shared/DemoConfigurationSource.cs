using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.ReverseProxy;

namespace OpenRastaDemo.Shared
{
  public class DemoConfigurationSource : IConfigurationSource
  {
    readonly string _reverseProxyUri;

    public DemoConfigurationSource(string reverseProxyUri = null)
    {
      ReverseProxyOptions = new ReverseProxyOptions();

      _reverseProxyUri = reverseProxyUri ?? "http://localhost/demoreverseproxy";
    }

    public ReverseProxyOptions ReverseProxyOptions { get; }

    public void Configure()
    {
      ResourceSpace.Uses
        .Hydra(options => options.Vocabulary = "https://schemas.example/schema#")
        .ReverseProxy(ReverseProxyOptions);

      ResourceSpace.Has
        .ResourcesOfType<IEnumerable<RootResponse>>()
        .AtUri("/")
        .Named("root")
        .HandledBy<RootHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json");

      ResourceSpace.Has
        .ResourcesOfType<RootResponse>()
        .AtUri("/littlejson")
        .Named("littlejson")
        .HandledBy<LittleJsonHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json");

      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri("/hydra")
        .EntryPointCollection()
        .HandledBy<HydraHandler>();

      ResourceSpace.Has
        .ResourcesOfType<HydraRootResponse>()
        .AtUri("/littlehydra")
        .HandledBy<LittleHydraHandler>();

      ResourceSpace.Has
        .ResourcesNamed("reverseproxy")
        .AtUri("/reverseproxy")
        .ReverseProxyFor(_reverseProxyUri);

      ResourceSpace.Has
        .ResourcesNamed("demoreverseproxy")
        .AtUri("/demoreverseproxy")
        .HandledBy<DemoReverseProxyHandler>();
    }
  }
}