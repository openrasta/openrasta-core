using InternalDependencyResolver_Specification;
using OpenRasta.DI;

namespace OpenRasta.Tests.Unit.DI
{
  public class registration_depending_on_func_of_unregistered_with_internal_dependency_resolver : dependency_resolver_context
  {
    public override IDependencyResolver CreateResolver()
    {
      return new InternalDependencyResolver();
    }
  }
}