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
      return Resolve(Registrations.GetRegistrationForService(serviceType));
    }

    public object Resolve(DependencyRegistration registration)
    {
      if (_recursionDefender.Contains(registration))
        throw new InvalidOperationException("Recursive dependencies are not allowed.");
      try
      {
        _recursionDefender.Push(registration);

        return registration.LifetimeManager.Resolve(this, registration);
      }
      finally
      {
        _recursionDefender.Pop();
      }
    }
  }
}