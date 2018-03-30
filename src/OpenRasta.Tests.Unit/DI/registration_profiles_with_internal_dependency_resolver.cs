using OpenRasta.DI;

namespace OpenRasta.Tests.Unit.DI
{
  public class registration_profiles_with_internal_dependency_resolver : registration_profiles
  {
    public override IDependencyResolver CreateResolver()
    {
      return new InternalDependencyResolver(); 
    }
  }
}