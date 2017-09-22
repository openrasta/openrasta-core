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
      return TryResolve(serviceType) ?? throw new DependencyResolutionException($"Could not find a resolve profile for {serviceType}");
    }

    public object TryResolve(Type serviceType)
    {
      return ResolveProfile.FindProfile(serviceType, this)?.TryResolve();
    }

    public T Resolve<T>(DependencyRegistration registration)
    {
      return (T) Resolve(registration);
    }

    private object Resolve(DependencyRegistration registration)
    {
      return TryResolve(registration)
        ?? throw new InvalidOperationException("Recursive dependencies are not allowed.");
    }

    public object TryResolve(DependencyRegistration registration)
    {
      if (_recursionDefender.Contains(registration))
        return null;
      try
      {
        _recursionDefender.Push(registration);
        return registration.ResolveInContext(this);
      }
      finally
      {
        _recursionDefender.Pop();
      }
    }
  }
}