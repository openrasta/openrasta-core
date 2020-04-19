using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;

namespace OpenRastaDemo.Benchmark.EndToEnd
{
  public class HealthCheckApi : IConfigurationSource, IDependencyResolverAccessor
  {
    class Resource
    {
      public string Status { get; set; }
    }

    class Handler
    {
      public Resource Get() => new Resource() {Status = "pass"};
    }

    public void Configure()
    {
      ResourceSpace.Has.ResourcesOfType<Resource>()
        .AtUri("/.well-known/health")
        .HandledBy<Handler>()
        .AsJsonNewtonsoft();
    }

    public IDependencyResolver Resolver { get; } = new WindsorDependencyResolver();
  }
}