using System;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  class EnumerableProfile<T> : ResolveProfile
  {
    readonly Func<IDependencyRegistrationCollection> _registrations;
    readonly ResolveContext _ctx;

    public EnumerableProfile(ResolveContext ctx, Func<IDependencyRegistrationCollection> registrations)
    {
      _ctx = ctx;
      _registrations = registrations;
    }

    public override bool TryResolve(out object instance)
    {
      var serviceTypeRegistrations = _registrations()[typeof(T)].ToArray();
      instance = null;
      var instances = new T[serviceTypeRegistrations.Length];
      for (var i = 0; i < serviceTypeRegistrations.Length; i++)
      {
        var profile = ResolveProfiles.Find(_ctx, serviceTypeRegistrations[i]);
        if (!profile(out var depInstance)) return false;
        instances[i] = (T) depInstance;
      }

      instance = instances;

      return true;
    }
  }
}