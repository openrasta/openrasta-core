using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class DependencyRegistrationCollection : IContextStoreDependencyCleaner
  {
    readonly Dictionary<Type, List<DependencyRegistration>> _registrations =
      new Dictionary<Type, List<DependencyRegistration>>();

    public IEnumerable<DependencyRegistration> this[Type serviceType]
    {
      get
      {
        lock (_registrations)
        {
          return GetOrCreateRegistrations(serviceType).ToList();
        }
      }
    }

    public void Add(DependencyRegistration registration)
    {
      lock (_registrations)
      {
        registration.VerifyRegistration(registration);
        GetOrCreateRegistrations(registration.ServiceType).Add(registration);
      }
    }

    public DependencyRegistration GetRegistrationForService(Type type)
    {
      lock (_registrations)
      {
        return GetOrCreateRegistrations(type).LastOrDefault(x => x.IsRegistrationAvailable(x));
      }
    }

    public bool HasRegistrationForService(Type type)
    {
      lock (_registrations)
      {
        return _registrations.ContainsKey(type) && GetOrCreateRegistrations(type)
                 .Any(x => x.IsRegistrationAvailable(x));
      }
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

    List<DependencyRegistration> GetOrCreateRegistrations(Type serviceType)
    {
      if (!_registrations.TryGetValue(serviceType, out var svcRegistrations))
        _registrations.Add(serviceType, svcRegistrations = new List<DependencyRegistration>());

      return svcRegistrations;
    }
  }
}