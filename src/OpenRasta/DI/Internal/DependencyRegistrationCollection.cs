using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class DependencyRegistrationCollection
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

    public DependencyRegistration LastRegistrationForService(Type serviceType)
    {
      return _registrations.TryGetValue(serviceType, out var regs)
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

    public object Resolve(ResolveContext ctx, Type serviceType)
    {
      return ctx.Resolve(LastRegistrationForService(serviceType));
    }

    public void Remove(DependencyRegistration transitiveRegistration)
    {
      if (_registrations.TryGetValue(transitiveRegistration.ServiceType, out var regs))
        regs.Remove(transitiveRegistration);
    }
  }
}