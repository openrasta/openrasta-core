using System;
using System.Collections.Generic;
using OpenRasta.DI.Internal;

namespace OpenRasta.DI
{
  public class RequestContextRegistrations : IDependencyRegistrationCollection
  {
    private readonly DependencyRegistrationCollection _globalRegistrations;
    private SingletonLifetimeManager _requestSingletons = new SingletonLifetimeManager();
    
    public RequestContextRegistrations(DependencyRegistrationCollection globalRegistrations)
    {
      _globalRegistrations = globalRegistrations;
    }

    public IEnumerable<DependencyRegistration> this[Type serviceType]
    {
      get { return _globalRegistrations[serviceType]; }
    }

    public void Add(DependencyRegistration registration)
    {
      _globalRegistrations.Add(registration);
    }

    public bool HasRegistrationForService(Type type)
    {
      return _globalRegistrations.HasRegistrationForService(type);
    }

    public bool TryResolve(ResolveContext ctx, Type serviceType, out object instance)
    {
      return _globalRegistrations.TryResolve(ctx, serviceType, out instance);
    }

    public void Remove(DependencyRegistration transitiveRegistration)
    {
      _globalRegistrations.Remove(transitiveRegistration);
    }
  }
}