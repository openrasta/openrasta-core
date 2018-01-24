using System;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_type : IDisposable
  {
    InMemoryHost host;

    public registering_type()
    {
      host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Dependency(context => context.Singleton<ClassWithDefaultConstructor>());
      });
    }

    [Fact]
    public void type_is_resolved()
    {
      host.Resolver.Resolve<ClassWithDefaultConstructor>().ShouldNotBeNull();
    }

    public void Dispose()
    {
      host.Close();
    }
  }
  public class registering_factory : IDisposable
  {
    InMemoryHost host;

    public registering_factory()
    {
      host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Dependency(context => context.Singleton(() => new ClassWithDefaultConstructor()));
      });
    }

    [Fact]
    public void type_is_resolved()
    {
      host.Resolver.Resolve<ClassWithDefaultConstructor>().ShouldNotBeNull();
    }


    public void Dispose()
    {
      ((IDisposable) host)?.Dispose();
    }
  }
}