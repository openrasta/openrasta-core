using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetContextStore : IContextStore
  {
    HttpContext Context => HttpContext.Current ?? ContextFromCallContext;
    HttpContext ContextFromCallContext => CallContext.LogicalGetData("__OR_CONTEXT") as HttpContext;

    public object this[string key]
    {
      get => Context.Items[key];
      set => Context.Items[key] = value;
    }

    public T GetOrAdd<T>(string key, Func<T> factory)
    {
      return Context.Items.Contains(key)
        ? (T)Context.Items[key]
        : (T)(Context.Items[key] = factory());
    }

    public bool TryGet<T>(string key, out T instance)
    {
      instance = default(T);
      
      if (!Context?.Items.Contains(key) != true ||
          !(Context.Items[key] is T typed))
        return false;
      
      instance = typed;
      return true;
    }

    public void Add<T>(string key, T instance)
    {
      Context.Items[key] = instance;
    }

    public void Remove(string key)
    {
      Context.Items.Remove(key);
    }
  }
}