using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class DependencyRegistrationCollection : IContextStoreDependencyCleaner
  {
    readonly ConcurrentDictionary<Type, List<DependencyRegistration>> _registrations =
      new ConcurrentDictionary<Type, List<DependencyRegistration>>();

    public IEnumerable<DependencyRegistration> this[Type serviceType] =>
      _registrations.TryGetValue(serviceType, out var result)
        ? result
        : Enumerable.Empty<DependencyRegistration>();

    public void Add(DependencyRegistration registration)
    {
      GetOrAddRegistrations(registration.ServiceType)
        .Add(registration);
    }

    public DependencyRegistration GetRegistrationForService(Type type)
    {
      return _registrations.TryGetValue(type, out var regs)
        ? regs.LastOrDefault(x => x.IsRegistrationAvailable)
        : null;
    }

    private List<DependencyRegistration> GetOrAddRegistrations(Type type)
    {
      return _registrations.GetOrAdd(type, t => new List<DependencyRegistration>());
    }

    public bool HasRegistrationForService(Type type)
    {
      return _registrations.TryGetValue(type, out var regs) 
             && regs.Any(x => x.IsRegistrationAvailable);
    }

    public void UnregisterTemporaryRegistration(DependencyRegistration registration, object instance)
    {
      if (!_registrations.TryGetValue(registration.ServiceType, out var match))
        return;
      match.RemoveAll(r =>r.IsInstanceRegistration && r.Key == registration.Key);
    }

    public object Resolve(ResolveContext ctx, Type serviceType)
    {
      return ctx.Resolve(GetRegistrationForService(serviceType));
    }
  }
}