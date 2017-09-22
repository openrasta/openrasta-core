using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace OpenRasta.Hosting
{
  public class AmbientContext
  {
    readonly Hashtable _items = new Hashtable();

    public static AmbientContext Current
    {
            get { return CallContext.HostContext as AmbientContext; }
    }

    public object this[string key]
    {
      get => _items[key];
      set => _items[key] = value;
    }

    public T GetOrAdd<T>(string key, Func<T> factory)
    {
      T result;
      if (_items.ContainsKey(key))
        result = (T) _items[key];
      else
        _items[key] = result = factory();
      return result;
    }

    public bool TryGet<T>(string key, out T instance)
    {
      var success = _items.ContainsKey(key);
      instance = success ? (T)_items[key] : default(T);
      return success;
    }

    public void Remove(string key)
    {
      _items.Remove(key);
    }
  }
}