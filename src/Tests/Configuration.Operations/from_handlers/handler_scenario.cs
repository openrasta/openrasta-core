using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public abstract class handler_scenario
  {
    protected IMetaModelRepository metamodel;

    protected void given_server_with_handler<T>()
    {
      using (var server = new InMemoryHost(() =>
      {
        ResourceSpace.Has.ResourcesNamed("test")
          .AtUri("/resource")
          .HandledBy<T>();
      }))
      {
        metamodel = server.Resolver.Resolve<IMetaModelRepository>();
      }
    }
  }
}