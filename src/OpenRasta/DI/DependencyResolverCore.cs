using System;
using System.Collections.Generic;

namespace OpenRasta.DI
{
  public abstract class DependencyResolverCore
  {
    public void AddDependency(Type serviceType, Type concreteType, DependencyLifetime lifetime)
    {
      CheckConcreteType(concreteType);
      CheckServiceType(serviceType, concreteType);
      CheckLifetime(lifetime);
      AddDependencyCore(serviceType, concreteType, lifetime);
    }

    public void AddDependency(Type concreteType, DependencyLifetime lifetime)
    {
      CheckConcreteType(concreteType);
      CheckLifetime(lifetime);

      AddDependencyCore(concreteType, lifetime);
    }

    protected abstract void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime);
    protected abstract void AddDependencyCore(Type concreteType, DependencyLifetime lifetime);

    public void AddDependencyInstance(Type serviceType, object instance, DependencyLifetime lifetime)
    {
      if (instance == null)
        throw new ArgumentNullException(nameof(instance));
      if (lifetime == DependencyLifetime.Transient)
        throw new ArgumentException(@"Cannot register an instance for Transient lifetimes.", nameof(lifetime));

      CheckServiceType(serviceType, instance.GetType());
      AddDependencyInstanceCore(serviceType, instance, lifetime);
    }

    protected abstract void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime);

    public object Resolve(Type serviceType)
    {
      if (serviceType == null)
        throw new ArgumentNullException(nameof(serviceType));
      try
      {
        return ResolveCore(serviceType);
      }
      catch (Exception e) when (!(e is DependencyResolutionException))
      {
        throw new DependencyResolutionException(
          $"An error occurred while trying to resolve type {serviceType.Name}.", e);
      }
    }

    public IEnumerable<TService> ResolveAll<TService>()
    {
      try
      {
        return ResolveAllCore<TService>();
      }
      catch (Exception e)
      {
        if (e is DependencyResolutionException)
          throw;
        throw new DependencyResolutionException(
          $"An error occurred while trying to resolve type {typeof(TService).Name}.", e);
      }
    }

    protected abstract IEnumerable<TService> ResolveAllCore<TService>();
    protected abstract object ResolveCore(Type serviceType);

    static void CheckConcreteType(Type concreteType)
    {
      if (concreteType == null)
        throw new ArgumentNullException(nameof(concreteType));
      if (concreteType.IsAbstract)
        throw new InvalidOperationException(
          $"The type {concreteType.FullName} is abstract. You cannot register an abstract type for initialization.");
    }

    static void CheckLifetime(DependencyLifetime lifetime)
    {
      if (!Enum.IsDefined(typeof(DependencyLifetime), lifetime))
        throw new InvalidOperationException(
          $"Value {lifetime} is unknown for enumeration DependencyLifetime.");
    }

    static void CheckServiceType(Type serviceType, Type concreteType)
    {
      if (serviceType == null)
        throw new ArgumentNullException(nameof(serviceType));
      if (concreteType == null)
        throw new ArgumentNullException(nameof(concreteType));
      if (!serviceType.IsAssignableFrom(concreteType))
        throw new InvalidOperationException(
          $"The type {concreteType.Name} doesn't implement or inherit from {serviceType.Name}.");
    }
  }
}