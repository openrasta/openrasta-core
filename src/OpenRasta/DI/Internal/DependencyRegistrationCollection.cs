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
      var regsForTypes = _registrations
        .GetOrAdd(registration.ServiceType, t => new List<DependencyRegistration>());
      regsForTypes.Add(registration);
    }

    public DependencyRegistration GetRegistrationForService(Type type)
    {
      return _registrations.TryGetValue(type, out var regs)
        ? regs.LastOrDefault(x => x.IsRegistrationAvailable(x))
        : null;
    }

    public bool HasRegistrationForService(Type type)
    {
      return _registrations.TryGetValue(type, out var regs) 
             && regs.Any(x => x.IsRegistrationAvailable(x));
    }

    public void Destruct(string key, object instance)
    {
      lock (_registrations)
      {
        foreach (var reg in _registrations)
        {
          var toRemove = reg.Value.Where(x => x.IsInstanceRegistration && x.Key == key).ToList();

          foreach (var x in toRemove)
          {
            reg.Value.Remove(x);
          }
        }
      }
    }
  }
}