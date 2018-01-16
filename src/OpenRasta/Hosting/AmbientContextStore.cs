using System;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting
{
  public class AmbientContextStore : IContextStore
  {
    public object this[string key]
    {
      get => AmbientContext.Current?[key];
      set => AmbientContext.Current[key] = value;
    }

    public T GetOrAdd<T>(string key, Func<T> factory)
    {
      return AmbientContext.Current.GetOrAdd(key, factory);
    }

    public bool TryGet<T>(string key, out T instance)
    {
      instance = default(T);
      return AmbientContext.Current != null && 
             AmbientContext.Current.TryGet(key, out instance);
    }

    public void Add<T>(string key, T instance)
    {
      AmbientContext.Current[key] = instance;
    }

    public void Remove(string key)
    {
      AmbientContext.Current.Remove(key);
    }
  }
}