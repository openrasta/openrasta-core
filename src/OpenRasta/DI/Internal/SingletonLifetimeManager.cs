using System;
using System.Collections.Generic;
using OpenRasta.Collections;

namespace OpenRasta.DI.Internal
{
  public class SingletonLifetimeManager : DependencyLifetimeManager
  {
    readonly IDictionary<string, object> _instances = new Dictionary<string, object>();

    public SingletonLifetimeManager(InternalDependencyResolver builder)
      : base(builder)
    {
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
        lock (_instances)
        {
          if (!_instances.TryGetValue(registration.Key, out var instance))
            _instances.Add(registration.Key, instance = base.Resolve(context, registration));
          return instance;
        }
    }

    public override void VerifyRegistration(DependencyRegistration registration)
    {
      if (!registration.IsInstanceRegistration) return;
      lock (_instances)
      {
        if (_instances.ContainsKey(registration.Key))
          throw new InvalidOperationException(
            "Trying to register an instance for a registration that already has one.");
        _instances[registration.Key] = registration.Instance;
        registration.Instance = null;
      }
    }
  }
}