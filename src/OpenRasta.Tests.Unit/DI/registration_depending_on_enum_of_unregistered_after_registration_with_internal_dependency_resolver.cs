using OpenRasta.DI;

namespace OpenRasta.Tests.Unit.DI
{
  public  class registration_depending_on_enum_of_unregistered_after_registration_with_internal_dependency_resolver : registration_depending_on_enum_of_unregistered_after_registration
  {
    public override IDependencyResolver CreateResolver() { return new InternalDependencyResolver(); }
  }
}