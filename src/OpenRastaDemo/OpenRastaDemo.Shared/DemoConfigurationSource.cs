using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Hydra;

namespace OpenRastaDemo
{
  public class DemoConfigurationSource : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Uses.Hydra(options => options.Vocabulary = "https://schemas.example/schema#");

      ResourceSpace.Has
        .ResourcesOfType<IEnumerable<RootResponse>>()
        .AtUri("/")
        .Named("root")
        .HandledBy<RootHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json")
        ;

      ResourceSpace.Has
        .ResourcesOfType<RootResponse>()
        .AtUri("/littlejson")
        .Named("littlejson")
        .HandledBy<LittleJsonHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>()
        .ForMediaType("application/json")
        ;

      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri("/hydra")
        .EntryPointCollection()
        .HandledBy<HydraHandler>();

      ResourceSpace.Has
        .ResourcesOfType<HydraRootResponse>()
        .AtUri("/littlehydra")
        .HandledBy<LittleHydraHandler>();
    }
  }
}