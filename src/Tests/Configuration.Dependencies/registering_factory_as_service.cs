using System;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_factory_as_service : IDisposable
  {
    InMemoryHost host;

    public registering_factory_as_service()
    {
      host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Dependency(ctx => 
          ctx.Register(() => new ClassWithDefaultConstructor())
            .As<IClassService>());
      });
    }

    [Fact]
    public void service_is_resolved()
    {
      host.Resolver.Resolve<IClassService>().ShouldNotBeNull();
    }

    public void Dispose()
    {
      ((IDisposable) host)?.Dispose();
    }
  }
}