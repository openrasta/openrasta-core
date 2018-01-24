using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_factory_with_2_args
  {
    readonly ClassWithArguments resolved;

    public registering_factory_with_2_args()
    {
      using (var host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.CustomDependency<ClassWithDefaultConstructor, ClassWithDefaultConstructor>();
        ResourceSpace.Uses.Dependency(context => context.Singleton(
          (ClassWithDefaultConstructor first, ClassWithDefaultConstructor second) =>
            new ClassWithArguments(first, second)));
      }))
      {
        resolved = host.Resolver.Resolve<ClassWithArguments>();
      }
    }

    [Fact]
    public void type_is_resolved_with_correct_dependencies()
    {
      resolved.ShouldHaveDependencies(2);
    }
  }
}