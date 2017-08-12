using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRasta.Collections;

namespace OpenRasta.DI.Internal
{
  public class SingletonLifetimeManager : DependencyLifetimeManager
  {
    readonly ConcurrentDictionary<string, object> _instances = new ConcurrentDictionary<string, object>();

    public SingletonLifetimeManager(InternalDependencyResolver builder)
      : base(builder)
    {
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      return _instances.GetOrAdd(registration.Key, key => CreateObject(context, registration));
    }

    public override void VerifyRegistration(DependencyRegistration registration)
    {
      if (!registration.IsInstanceRegistration) return;
      
      if (!_instances.TryAdd(registration.Key, registration.Instance))
        throw new InvalidOperationException(
          "Trying to register an instance for a registration that already has one.");
      registration.Instance = null;
    }
  }
}