using System;
using System.Collections.Generic;

namespace OpenRasta.DI
{
  class CompatiblityScopedResolver : IDependencyResolver, IDisposable
  {
    private readonly IDependencyResolver _dependencyResolverImplementation;

    public CompatiblityScopedResolver(IDependencyResolver dependencyResolverImplementation)
    {
      _dependencyResolverImplementation = dependencyResolverImplementation;
    }

    public bool HasDependency(Type serviceType)
    {
      return _dependencyResolverImplementation.HasDependency(serviceType);
    }

    public bool HasDependencyImplementation(Type serviceType, Type concreteType)
    {
      return _dependencyResolverImplementation.HasDependencyImplementation(serviceType, concreteType);
    }

    public void AddDependency(Type concreteType, DependencyLifetime lifetime)
    {
      _dependencyResolverImplementation.AddDependency(concreteType, lifetime);
    }

    public void AddDependency(Type serviceType, Type concreteType, DependencyLifetime dependencyLifetime)
    {
      _dependencyResolverImplementation.AddDependency(serviceType, concreteType, dependencyLifetime);
    }

    public void AddDependencyInstance(Type registeredType, object value, DependencyLifetime dependencyLifetime)
    {
      _dependencyResolverImplementation.AddDependencyInstance(registeredType, value, dependencyLifetime);
    }

    public IEnumerable<TService> ResolveAll<TService>()
    {
      return _dependencyResolverImplementation.ResolveAll<TService>();
    }

    public object Resolve(Type type)
    {
      return _dependencyResolverImplementation.Resolve(type);
    }

    public void HandleIncomingRequestProcessed()
    {
      _dependencyResolverImplementation.HandleIncomingRequestProcessed();
    }

    public void Dispose()
    {
      HandleIncomingRequestProcessed();
    }
  }
}