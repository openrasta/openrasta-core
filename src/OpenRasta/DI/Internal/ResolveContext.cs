using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  public class ResolveContext
  {
    readonly Stack<DependencyRegistration> _recursionDefender = new Stack<DependencyRegistration>();

    public ResolveContext(IDependencyRegistrationCollection registrations)
    {
      Registrations = registrations;
    }

    public IDependencyRegistrationCollection Registrations { get; }

    public bool TryResolve(Type serviceType, out object instance)
    {
      instance = null;
      var profile = ResolveProfile.FindProfile(serviceType, this);

      return profile != null && profile.TryResolve(out instance);
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