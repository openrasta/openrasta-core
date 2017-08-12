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
      Builder = new ObjectBuilder(this);
    }

    public ObjectBuilder Builder { get; }
    public DependencyRegistrationCollection Registrations { get; }

    public bool CanResolve(DependencyRegistration registration)
    {
      return !_recursionDefender.Contains(registration);
    }

    public object Resolve(Type serviceType)
    {
      var resolveProfile = ResolveProfile.FindProfile(serviceType, this)
                           ?? throw new DependencyResolutionException($"Could not find a resolve profile for {serviceType}");
      
      return resolveProfile.Resolve();
    }

    public object TryResolve(Type serviceType)
    {
      return ResolveProfile.FindProfile(serviceType, this)?.Resolve();
    }

    public T Resolve<T>(DependencyRegistration registration)
    {
      return (T) Resolve(registration);
    }

    public object Resolve(DependencyRegistration registration)
    {
      if (_recursionDefender.Contains(registration))
        throw new InvalidOperationException("Recursive dependencies are not allowed.");
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