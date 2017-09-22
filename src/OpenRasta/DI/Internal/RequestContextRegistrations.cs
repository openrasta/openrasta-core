using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  public class RequestContextRegistrations : IDependencyRegistrationCollection, IDisposable
  {
    private readonly GlobalRegistrations _globalRegistrations;

    readonly ConcurrentDictionary<Type, RegistrationBag> _registrations =
      new ConcurrentDictionary<Type, RegistrationBag>();

    private readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimes;

    public RequestContextRegistrations(GlobalRegistrations globalRegistrations)
    {
      _globalRegistrations = globalRegistrations;

      var singleton = new SingletonLifetimeManager();
      _lifetimes = new Dictionary<DependencyLifetime, DependencyLifetimeManager>(3)
      {
        {DependencyLifetime.PerRequest, singleton},
        {DependencyLifetime.Singleton, singleton},
        {DependencyLifetime.Transient, new TransientLifetimeManager()}
      };
    }

    public IEnumerable<DependencyRegistration> this[Type serviceType] =>
      _registrations.TryGetValue(serviceType, out var bag)
        ? bag.All.Concat(_globalRegistrations[serviceType]).ToArray()
        : _globalRegistrations[serviceType];

    public void Add(DependencyRegistration registration)
    {
      var bag = _registrations.GetOrAdd(registration.ServiceType, type => new RegistrationBag());
      bag.Add(new DependencyRegistration(
        registration.ServiceType,
        registration.ConcreteType,
        registration.Lifetime,
        _lifetimes[registration.Lifetime],
        registration.Instance));
    }

    public bool HasRegistrationForService(Type type)
    {
      return _registrations.ContainsKey(type) || _globalRegistrations.HasRegistrationForService(type);
    }

    public bool TryResolve(ResolveContext ctx, Type serviceType, out object instance)
    {
      return _registrations.TryGetValue(serviceType, out var bag)
        ? ctx.TryResolve(bag.Last, out instance)
        : _globalRegistrations.TryResolve(ctx, serviceType, out instance);
    }

    public void Remove(DependencyRegistration transitiveRegistration)
    {
      if (_registrations.TryGetValue(transitiveRegistration.ServiceType, out var bag)
          && bag.TryRemove(transitiveRegistration, out var isEmpty))
      {
        if (isEmpty)
        {
          _registrations.TryRemove(transitiveRegistration.ServiceType, out _);
        }
      }
      else
      {
        _globalRegistrations.Remove(transitiveRegistration);
      }
    }

    public void Dispose()
    {
      
    }
  }
}