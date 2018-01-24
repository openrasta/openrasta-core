using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_factory_with_1_arg
  {
    readonly ClassWithArguments resolved;

    public registering_factory_with_1_arg()
    {
      using (var host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.CustomDependency<ClassWithDefaultConstructor, ClassWithDefaultConstructor>();
        ResourceSpace.Uses.Dependency(context =>
          context.Singleton(
            (ClassWithDefaultConstructor first) => new ClassWithArguments(first)));
      }))
      {
        resolved = host.Resolver.Resolve<ClassWithArguments>();
      }
    }

    [Fact]
    public void type_is_resolved_with_correctr_dependencies()
    {
      resolved.ShouldHaveDependencies(1);
    }
  }
}