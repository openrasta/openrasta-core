using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class GlobalRegistrations : IDependencyRegistrationCollection
  {
    readonly ConcurrentDictionary<Type, RegistrationBag> _registrations =
      new ConcurrentDictionary<Type, RegistrationBag>();
    
    public IEnumerable<DependencyRegistration> this[Type serviceType] =>
      _registrations.TryGetValue(serviceType, out var bag)
        ? bag.All
        : Enumerable.Empty<DependencyRegistration>();

    public void Add(DependencyRegistration registration)
    {
      _registrations
        .GetOrAdd(registration.ServiceType, t => new RegistrationBag())
        .Add(registration);
    }

    public DependencyRegistration DefaultRegistrationFor(Type serviceType)
    {
      return _registrations.TryGetValue(serviceType, out var regs)
        ? regs.Last
        : null;
    }

    public bool HasRegistrationForService(Type type)
    {
      return _registrations.ContainsKey(type);
    }
  }
}