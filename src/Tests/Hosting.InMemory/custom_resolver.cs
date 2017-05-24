using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Shouldly;
using Xunit;

namespace Tests.Hosting.InMemory
{
  public class custom_resolver
  {
    InMemoryHost _host;
    CustomResolver _customResolver;

    public custom_resolver()
    {
      _customResolver = new CustomResolver();
      _host = new InMemoryHost(()=>{}, _customResolver);
    }

    [Fact]
    public void the_resolver_is_a_custom_dependency_resolver()
    {
      _host.Resolver.ShouldBe(_customResolver);
    }

    class CustomResolver : InternalDependencyResolver
    {
    }
  }
}