using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.DI.Windsor;
using OpenRasta.Web;

namespace OpenRastaDemo.Benchmark.EndToEnd
{
  public class SimpleApi : IConfigurationSource, IDependencyResolverAccessor
  {
    class Event
    {
      public int Id { get; set; }


      public class Handler
      {
        static ConcurrentDictionary<int, Event> _store = new ConcurrentDictionary<int, Event>()
        {
          [1] = new Event {Id = 1},
          [2] = new Event {Id = 2}
        };
        public Task<IEnumerable<Event>> Get() => Task.FromResult(_store.Values.AsEnumerable());
        public async Task<OperationResult> Get(int id)
        {
          return _store.TryGetValue(id, out var ev) 
            ? (OperationResult)new OperationResult.OK(ev) 
            : new OperationResult.NotFound();
        }
      }
    }
    class HealthCheck
    {
      public string Status { get; set; }

      public class Handler
      {
        public HealthCheck Get() => new HealthCheck() {Status = "pass"};
      }
    }

    public void Configure()
    {
      ResourceSpace.Has.ResourcesOfType<HealthCheck>()
        .AtUri("/.well-known/health")
        .HandledBy<HealthCheck.Handler>()
        .AsJsonNewtonsoft();

      ResourceSpace.Has.ResourcesOfType<Event>()
        .AtUri(r => $"/events/{r.Id}")
        .HandledBy<Event.Handler>()
        .AsJsonNewtonsoft();

      ResourceSpace.Has.ResourcesOfType<IEnumerable<Event>>()
        .AtUri("/events/")
        .HandledBy<Event.Handler>()
        .AsJsonNewtonsoft();
    }

    public IDependencyResolver Resolver { get; } = new WindsorDependencyResolver();
  }
}