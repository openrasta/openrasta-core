using System;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_factory : IDisposable
  {
    InMemoryHost host;

    public registering_factory()
    {
      host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Dependency(context => context.Register(() => new ClassWithDefaultConstructor()));
      });
    }

    [Fact]
    public void type_is_resolved()
    {
      host.Resolver.Resolve<ClassWithDefaultConstructor>().ShouldNotBeNull();
    }

    class ClassWithDefaultConstructor
    {
    }

    public void Dispose()
    {
      ((IDisposable) host)?.Dispose();
    }
  }
}