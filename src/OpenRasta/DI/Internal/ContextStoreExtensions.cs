using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public static class ContextStoreExtensions
  {
    const string CTX_INSTANCES_KEY = "__OR_CTX_INSTANCES_KEY";
    private const string CTX_INSTANCES_KEY_CONCURRENT = nameof(CTX_INSTANCES_KEY_CONCURRENT);

    public static void Destruct(this IContextStore store)
    {
      var allInstances = store.GetContextInstances();
      lock (allInstances)
      {
        foreach (var dep in allInstances)
          dep.Cleanup();
        allInstances.Clear();
      }
    }

    public static IList<ContextStoreDependency> GetContextInstances(this IContextStore store)
    {
      lock (store)
      {
        return (IList<ContextStoreDependency>)
          (store[CTX_INSTANCES_KEY] ?? (store[CTX_INSTANCES_KEY] = new List<ContextStoreDependency>()));
      }
    }
    
    
    public static ConcurrentDictionary<DependencyRegistration, object> GetConcurrentContextInstances(this IContextStore store)
    {
      lock (store)
      {
        return (ConcurrentDictionary<DependencyRegistration, object>)
        (store[CTX_INSTANCES_KEY_CONCURRENT] ??
         (store[CTX_INSTANCES_KEY_CONCURRENT] = new ConcurrentDictionary<DependencyRegistration, object>()));
      }
    }
  }
}