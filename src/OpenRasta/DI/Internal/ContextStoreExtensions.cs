using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public static class ContextStoreExtensions
  {
    const string CTX_INSTANCES_KEY = "__OR_CTX_INSTANCES_KEY";

    public static void Destruct(this IContextStore store)
    {
      var allInstances = store.GetContextInstances();
      lock (allInstances)
      {
        foreach (var dep in allInstances)
          dep.Cleaner?.Destruct(dep.Key, dep.Instance);
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
  }
}