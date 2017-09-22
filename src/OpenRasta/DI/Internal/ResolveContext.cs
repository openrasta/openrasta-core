using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  public class ResolveContext
  {
    readonly Stack<DependencyRegistration> _recursionDefender = new Stack<DependencyRegistration>();

    public ResolveContext(DependencyRegistrationCollection registrations)
    {
      Registrations = registrations;
    }

    public DependencyRegistrationCollection Registrations { get; }

    public object Resolve(Type serviceType)
    {
      return TryResolve(serviceType, out var instance)
        ? instance
        : throw new DependencyResolutionException($"Could not find a resolve profile for {serviceType}");
    }

    public bool TryResolve(Type serviceType, out object instance)
    {
      instance = null;
      var profile = ResolveProfile.FindProfile(serviceType, this);

      return profile != null && profile.TryResolve(out instance);
    }

    public T Resolve<T>(DependencyRegistration registration)
    {
      return (T) Resolve(registration);
    }

    private object Resolve(DependencyRegistration registration)
    {
      return TryResolve(registration, out var instance)
        ? instance
        : throw new InvalidOperationException("Recursive dependencies are not allowed.");
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