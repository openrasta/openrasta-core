using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OpenRasta.Collections;

namespace OpenRasta.DI.Internal
{
  public class SingletonLifetimeManager : DependencyLifetimeManager
  {
    readonly ConcurrentDictionary<string, Lazy<object>> _instances = new ConcurrentDictionary<string, Lazy<object>>();

    public SingletonLifetimeManager(InternalDependencyResolver builder)
      : base(builder)
    {
    }

    public override bool Contains(DependencyRegistration registration)
    {
      return registration.LifetimeManager == this;
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      var lazy = _instances
        .GetOrAdd(registration.Key, key => ThreadSafeLazyFactory(context, registration));
      return lazy.Value;
    }

    private static Lazy<object> ThreadSafeLazyFactory(ResolveContext context, DependencyRegistration registration)
    {
      return new Lazy<object>(() => registration.CreateInstance(context), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public override void Add(DependencyRegistration registration)
    {
    }

    public override void ClearScope()
    {
      
    }
  }
}