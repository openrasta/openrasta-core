using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  public class RequestContextRegistrations : IDependencyRegistrationCollection, IDisposable
  {
    readonly GlobalRegistrations _globalRegistrations;

    readonly ConcurrentDictionary<Type, RegistrationBag> _registrations =
      new ConcurrentDictionary<Type, RegistrationBag>();

    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimes;

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
      bag.Add(registration.OverrideLifetimeManager(_lifetimes[registration.Lifetime]));
    }

    public bool HasRegistrationForService(Type type)
    {
      return _registrations.ContainsKey(type) || _globalRegistrations.HasRegistrationForService(type);
    }

    
    public DependencyRegistration DefaultRegistrationFor(Type serviceType)
    {
      return _registrations.TryGetValue(serviceType, out var regs)
        ? regs.Last
        : _globalRegistrations.DefaultRegistrationFor(serviceType);
    }

    public void Dispose()
    {
      foreach (var lifetime in _lifetimes.Values)
      {
        lifetime.EndScope();
      }
    }
  }
}