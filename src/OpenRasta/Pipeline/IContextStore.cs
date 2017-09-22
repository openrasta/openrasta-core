using System;
using OpenRasta.DI;

namespace OpenRasta.Pipeline
{
  public interface IContextStore
  {
    T GetOrAdd<T>(string key, Func<T> factory);
    bool TryGet<T>(string key, out T instance);
    void Add<T>(string key, T instance);
    void Remove(string key);
  }
}