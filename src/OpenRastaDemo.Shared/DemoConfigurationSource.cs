using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra;
using Tests.Plugins.Hydra.Utf8Json;

namespace OpenRastaDemo
{
  public class HydraApi : IConfigurationSource
  {
    readonly bool _precompile;
    List<HydraRootResponse> _data;

    public HydraApi(bool precompile, List<HydraRootResponse> data)
    {
      _precompile = precompile;
      _data = data;
    }

    public void Configure()
    {
      ResourceSpace.Uses.Hydra(options =>
      {
        options.Vocabulary = "https://schemas.example/schema#";
        if (_precompile)
        {
          options.Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
        }
      });

      ResourceSpace.Uses.Dependency(ctx => ctx.Transient(() => new HydraHandler(_data)));

      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri("/hydra")
        .EntryPointCollection()
        .HandledBy<HydraHandler>()
        .AsJsonNewtonsoft();
      
      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri(r => "/hydrafasturi")
        .HandledBy<HydraHandler>();

      ResourceSpace.Has
        .ResourcesOfType<HydraRootResponse>()
        .AtUri("/littlehydra")
        .HandledBy<LittleHydraHandler>()
        .AsJsonNewtonsoft();
      ;
    }
  }

  public class DemoConfigurationSource : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Uses.Hydra(options => { options.Vocabulary = "https://schemas.example/schema#"; });

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