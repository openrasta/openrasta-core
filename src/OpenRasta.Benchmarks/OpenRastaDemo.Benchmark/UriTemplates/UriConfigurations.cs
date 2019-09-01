using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;

namespace OpenRastaDemo.Benchmark.UriTemplates
{
  public class UriConfigurations : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Has.ResourcesOfType<EventMappedWithStrings>()
        .AtUri("/event/ids/{id}")
        .HandledBy<EventMappedWithStrings.Handler>()
        .AsJsonNewtonsoft();
    }
  }
}