using System;
using System.Collections.Generic;

namespace OpenRasta.DI
{
  public interface IDependencyResolver
  {
    bool HasDependency(Type serviceType);

    bool HasDependencyImplementation(Type serviceType, Type concreteType);

    void AddDependency(Type concreteType, DependencyLifetime lifetime);

    void AddDependency(Type serviceType, Type concreteType, DependencyLifetime dependencyLifetime);

    void AddDependencyInstance(Type registeredType, object value, DependencyLifetime dependencyLifetime);

    IEnumerable<TService> ResolveAll<TService>();

    /// <summary>
    /// Resolves an instance of a type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>An instance of the type.</returns>
    object Resolve(Type type);

    void HandleIncomingRequestProcessed();
  }
}