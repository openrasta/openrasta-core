using System;
using System.Collections.Generic;

namespace OpenRasta.Plugins.Hydra
{
  static class DictionaryExtensions
  {
    public static T GetOrAdd<T>(this IDictionary<string, object> dictionary, string key) where T : new()
    {
      return dictionary.GetOrAdd(key, () => new T());
    }

    public static T GetOrAdd<T>(this IDictionary<string, object> dictionary, string key, Func<T> value)
    {
      if (!dictionary.TryGetValue(key, out var hydraModel))
        dictionary[key] = hydraModel = value();

      return (T) hydraModel;
    }
  }
}