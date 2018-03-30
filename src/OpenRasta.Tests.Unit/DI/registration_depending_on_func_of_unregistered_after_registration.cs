using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.DI;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public abstract class registration_depending_on_func_of_unregistered_after_registration : dependency_resolver_context
  {
    DependsOnFuncOfSimple instance;

    [Test]
    public void func_is_resolved()
    {
      Resolver.AddDependency<DependsOnFuncOfSimple>();
      instance = Resolver.Resolve<DependsOnFuncOfSimple>();
      Resolver.AddDependency<Simple>();

      instance.Simple().ShouldNotBeNull();
    }
  }
}