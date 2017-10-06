using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using OpenRasta.DI.Internal;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.TypeSystem.ReflectionBased;

namespace OpenRasta.DI
{
  public class InternalDependencyResolver : DependencyResolverCore, IDependencyResolver
  {
    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimeManagers;
    ILogger _log = TraceSourceLogger.Instance;

    public InternalDependencyResolver()
    {
      Registrations = new DependencyRegistrationCollection();
      _lifetimeManagers = new Dictionary<DependencyLifetime, DependencyLifetimeManager>
      {
        {DependencyLifetime.Transient, new TransientLifetimeManager(this)},
        {DependencyLifetime.Singleton, new SingletonLifetimeManager(this)},
        {DependencyLifetime.PerRequest, new PerRequestLifetimeManager(this)}
      };
    }

    public ILogger Log
    {
      get => _log;
      set => _log = value;
    }

    public DependencyRegistrationCollection Registrations { get; private set; }

    protected override void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime)
    {
      Registrations.Add(new DependencyRegistration(serviceType, concreteType, _lifetimeManagers[lifetime]));
    }

    protected override void AddDependencyCore(Type concreteType, DependencyLifetime lifetime)
    {
      AddDependencyCore(concreteType, concreteType, lifetime);
    }

    protected override void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime)
    {
      var instanceType = instance.GetType();

      var registration = new DependencyRegistration(serviceType, instanceType, _lifetimeManagers[lifetime], instance);

      Registrations.Add(registration);
    }

    protected override IEnumerable<TService> ResolveAllCore<TService>()
    {
//      return from dependency in Registrations[typeof(TService)]
//        where dependency.LifetimeManager.IsRegistrationAvailable(dependency)
//        select (TService) Resolve(dependency);
      return ((IEnumerable<object>) ResolveCore(typeof(IEnumerable<TService>))).Cast<TService>();
    }

    protected override object ResolveCore(Type serviceType)
    {
      return (
          ResolveProfile.Simple(Registrations, serviceType)
          ?? ResolveProfile.Enumerable(Registrations, serviceType)
          ?? throw new DependencyResolutionException($"No type registered for {serviceType.Name}")
      ).Resolve();
    }

    abstract class ResolveProfile
    {
      public static ResolveProfile Simple(DependencyRegistrationCollection registrations, Type serviceType)
      {
        return registrations.HasRegistrationForService(serviceType)
          ? new SimpleProfile(registrations, serviceType)
          : null;
    }

      public static ResolveProfile Enumerable(DependencyRegistrationCollection registrations, Type serviceType)
      {
        var enumType = serviceType.FindInterface(typeof(IEnumerable<>));
        if (enumType == null) return null;

        var innerServiceType = enumType.GenericTypeArguments[0];

        return (ResolveProfile) Activator
          .CreateInstance(typeof(EnumerableProfile<>)
            .MakeGenericType(innerServiceType), registrations);
      }

      public abstract object Resolve();
    }

    class SimpleProfile : ResolveProfile
    {
      readonly DependencyRegistrationCollection _registrations;
      readonly Type _serviceType;

      public SimpleProfile(DependencyRegistrationCollection registrations, Type serviceType)
      {
        _registrations = registrations;
        _serviceType = serviceType;
      }

      public override object Resolve()
      {
        return new ResolveContext(_registrations)
          .Resolve(_registrations.GetRegistrationForService(_serviceType));
      }
    }

    class EnumerableProfile<T> : ResolveProfile
    {
      readonly DependencyRegistrationCollection _registrations; 
      public EnumerableProfile(DependencyRegistrationCollection registrations)
      {
        _registrations = registrations;
      }
      public override object Resolve()
      {
        if (!_registrations.HasRegistrationForService(typeof(T)))
          return System.Linq.Enumerable.Empty<T>();
        return (
          from dependency in _registrations[typeof(T)]
          where dependency.LifetimeManager.IsRegistrationAvailable(dependency)
          select (T)new ResolveContext(_registrations).Resolve(dependency)
        ).ToList();
      }
    }

    public void HandleIncomingRequestProcessed()
    {
      var store = (IContextStore) Resolve(typeof(IContextStore));
      store.Destruct();
    }

    public bool HasDependency(Type serviceType)
    {
      return serviceType != null && Registrations.HasRegistrationForService(serviceType);
    }

    public bool HasDependencyImplementation(Type serviceType, Type concreteType)
    {
            return Registrations.HasRegistrationForService(serviceType) && Registrations[serviceType].Count(r => r.ConcreteType == concreteType) >= 1;
    }

    object Resolve(DependencyRegistration dependency)
    {
      return new ResolveContext(Registrations).Resolve(dependency);
    }
  }
}
