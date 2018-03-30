using OpenRasta.DI;

namespace OpenRasta.Tests.Unit.DI
{
  public class registration_depending_on_func_in_other_scope_with_internal_dependency_resolver: registration_depending_on_func_in_other_scope
  {
    public override IDependencyResolver CreateResolver() { return new InternalDependencyResolver(); }
  }
}