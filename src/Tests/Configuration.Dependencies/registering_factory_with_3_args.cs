using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using Xunit;

namespace Tests.Configuration.Dependencies
{
  public class registering_factory_with_3_args
  {
    readonly ClassWithArguments resolved;

    public registering_factory_with_3_args()
    {
      using (var host = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.CustomDependency<ClassWithDefaultConstructor, ClassWithDefaultConstructor>();
        ResourceSpace.Uses.Dependency(context => context.Singleton(
          (ClassWithDefaultConstructor first,
            ClassWithDefaultConstructor second,
            ClassWithDefaultConstructor third) => new ClassWithArguments(first, second, third)));
      }))
      {
        resolved = host.Resolver.Resolve<ClassWithArguments>();
      }
    }

    [Fact]
    public void type_is_resolved_with_correct_dependencies()
    {
      resolved.ShouldHaveDependencies(3);
    }
  }
}