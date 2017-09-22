using System;
using OpenRasta.Collections;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.InMemory
{
    public class InMemoryContextStore : NullBehaviorDictionary<string, object>, IContextStore
    {
      public T GetOrAdd<T>(string key, Func<T> factory)
      {
        T instance;
        if (ContainsKey(key))
          instance = (T) base[key];
        else
          base[key] = instance = factory();
        return instance;
      }

      public bool TryGet<T>(string key, out T instance)
      {
        var hasKey = ContainsKey(key);
        instance = hasKey ? (T) base[key] : default(T);
        return hasKey;
      }

      public void Add<T>(string key, T instance)
      {
        base[key] = instance;
      }
    }
}