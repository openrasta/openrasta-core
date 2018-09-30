using System.Collections.Generic;
using Newtonsoft.Json;
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
    string _data;

    public HydraApi(bool precompile, string data)
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

      ResourceSpace.Uses.Dependency(ctx => ctx.Transient(() => new HydraHandler<HydraRootResponse>(JsonConvert.DeserializeObject<List<HydraRootResponse>>(_data))));
      ResourceSpace.Uses.Dependency(ctx => ctx.Transient(() => new HydraHandler<FastUriResponse>(JsonConvert.DeserializeObject<List<FastUriResponse>>(_data))));

      ResourceSpace.Has
        .ResourcesOfType<List<HydraRootResponse>>()
        .AtUri("/hydra")
        .HandledBy<HydraHandler<HydraRootResponse>>()
        .AsJsonNewtonsoft();
      
      ResourceSpace.Has
        .ResourcesOfType<HydraRootResponse>()
        .AtUri("/hydra/{_id}")
        .HandledBy<HydraHandler<HydraRootResponse>>()
        .AsJsonNewtonsoft();
      
      
      ResourceSpace.Has
        .ResourcesOfType<List<FastUriResponse>>()
        .AtUri(r => "/hydrafasturi")
        .HandledBy<HydraHandler<FastUriResponse>>();

      ResourceSpace.Has
        .ResourcesOfType<FastUriResponse>()
        .AtUri(r => $"/hydrafasturi/{r._id}")
        .HandledBy<HydraHandler<FastUriResponse>>();
      
      ResourceSpace.Has
        .ResourcesOfType<LittleHydraHandler>()
        .AtUri("/littlehydra")
        .HandledBy<LittleHydraHandler>()
        .AsJsonNewtonsoft();
      ;
    }
  }
}