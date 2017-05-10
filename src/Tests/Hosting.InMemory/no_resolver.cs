using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Hosting.InMemory
{
  public class no_resolver
  {
    readonly InMemoryHost _host;

    public no_resolver()
    {
      _host = new InMemoryHost(null);
    }

    [Fact]
    public void resolver_is_an_internal_dependency_resolver()
    {
      _host.Resolver.ShouldNotBeNull();
      _host.Resolver.ShouldBeOfType<InternalDependencyResolver>();
    }
  }
}