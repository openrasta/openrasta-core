using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI.Internal;
using OpenRasta.Pipeline;

namespace OpenRasta.DI
{
  public class InternalDependencyResolver : DependencyResolverCore, IDependencyResolver
  {
    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimeManagers;

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

    public DependencyRegistrationCollection Registrations { get; }

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
      return ((IEnumerable<object>) ResolveCore(typeof(IEnumerable<TService>))).Cast<TService>();
    }

    protected override object ResolveCore(Type serviceType)
    {
      try
    {
        return new ResolveContext(Registrations).Resolve(serviceType);
      }
      catch (Exception e)
      {
        throw new DependencyResolutionException($"Could not resolve dependencies for {serviceType}", e);
      }
    }

    public void HandleIncomingRequestProcessed()
    {
      var store = (IContextStore) Resolve(typeof(IContextStore));
      lock (store)
      {
      store.Destruct();
    }
    }

    public bool HasDependency(Type serviceType)
    {
      return serviceType != null && Registrations.HasRegistrationForService(serviceType);
    }

    public bool HasDependencyImplementation(Type serviceType, Type concreteType)
    {
            return Registrations.HasRegistrationForService(serviceType) && Registrations[serviceType].Count(r => r.ConcreteType == concreteType) >= 1;
    }
  }
}