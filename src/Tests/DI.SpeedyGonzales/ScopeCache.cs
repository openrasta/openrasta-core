using System;
using System.Collections.Generic;

namespace Tests.DI.SpeedyGonzales
{
  public class ScopeCache
  {
    readonly Dictionary<Type, Func<object>> _instances = new Dictionary<Type, Func<object>>();

    public bool TryGetInstance<T>(out T instance)
    {
      if (_instances.TryGetValue(typeof(T), out var factory))
      {
        instance = (T) factory();
        return true;
      }

      instance = default;
      return false;
    }

    public void StoreInstance(Type serviceType, Func<object> instance)
    {
      _instances.Add(serviceType, instance);
    }
  }
}