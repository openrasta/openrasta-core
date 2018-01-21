using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  delegate bool ProfileResolver(out object instance);
  
  public class ResolveContext
  {
    readonly Stack<DependencyRegistration> _recursionDefender = new Stack<DependencyRegistration>();

    public ResolveContext(Func<IDependencyRegistrationCollection> registrations)
    {
      Registrations = registrations;
    }

    Func<IDependencyRegistrationCollection> Registrations { get; }

    public bool TryResolve(Type serviceType, out object instance)
    {
      instance = null;
      var profile = ResolveProfiles.Find(serviceType, this, Registrations);

      return profile != null && profile(out instance);
    }

    public bool TryResolve(DependencyRegistration registration, out object instance)
    {
      instance = null;
      if (_recursionDefender.Contains(registration))
        return false;
      try
      {
        _recursionDefender.Push(registration);
        instance = registration.ResolveInContext(this);
        return true;
      }
      finally
      {
        _recursionDefender.Pop();
      }
    }

  }
}