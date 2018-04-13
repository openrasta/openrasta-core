using System.Linq;

namespace OpenRasta.DI.Internal
{
  class EnumerableProfile<T> : ResolveProfile
  {
    public override bool TryResolve(IDependencyRegistrationCollection registrations, ResolveContext resolveContext, out object instance)
    {
      var serviceTypeRegistrations = registrations[typeof(T)].ToArray();
      instance = null;
      var instances = new T[serviceTypeRegistrations.Length];
      for (var i = 0; i < serviceTypeRegistrations.Length; i++)
      {
        var profile = ResolveProfiles.Find(resolveContext, serviceTypeRegistrations[i]);
        if (!profile(registrations, resolveContext, out instance))
          throw new DependencyResolutionException($"Could not resolve an enumerable for {serviceTypeRegistrations[i].ConcreteType}");
        instances[i] = (T) instance;
      }

      instance = instances;

      return true;
    }
  }
}