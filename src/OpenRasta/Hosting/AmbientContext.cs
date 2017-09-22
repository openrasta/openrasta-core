using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace OpenRasta.Hosting
{
  public class AmbientContext
  {
    readonly Hashtable _items = new Hashtable();

    private static readonly AsyncLocal<AmbientContext> _current = new AsyncLocal<AmbientContext>();

    public static AmbientContext Current
    {
      get => _current.Value;
      set => _current.Value = value;
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
  }
}