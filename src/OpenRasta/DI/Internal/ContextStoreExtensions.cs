using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  [Obsolete("Internal class no longer in use.")]
  public class ContextStoreDependency
  {
    public ContextStoreDependency(string key, object instance, IContextStoreDependencyCleaner cleaner)
    {
      if (key == null) throw new ArgumentNullException("key");
      if (instance == null) throw new ArgumentNullException("instance");
      Key = key;
      Instance = instance;
      Cleaner = cleaner;
    }

    public IContextStoreDependencyCleaner Cleaner { get; set; }
    public object Instance { get; set; }
    public string Key { get; set; }
  }
  
  public static class ContextStoreExtensions
  {
    private const string CTX_INSTANCES_KEY_CONCURRENT = nameof(CTX_INSTANCES_KEY_CONCURRENT);

    
    [Obsolete("Internal no longer in use.")]
    public static void Destruct(this IContextStore store)
    {
      foreach (var dep in store.GetContextInstances())
          dep.Cleaner?.Destruct(dep.Key, dep.Instance);
      store.GetContextInstances().Clear();
    }
    
    [Obsolete("Internal no longer in use.")]
    public static IList<ContextStoreDependency> GetContextInstances(this IContextStore store)
    {
      return (IList<ContextStoreDependency>)
        (store["__OR_CTX_INSTANCES_KEY"] ?? (store["__OR_CTX_INSTANCES_KEY"] = new List<ContextStoreDependency>()));
    }
    
    public static ConcurrentDictionary<DependencyRegistration, object> GetConcurrentContextInstances(
      this IContextStore store)
    {
      lock (store)
      {
        if (!(store[CTX_INSTANCES_KEY_CONCURRENT] is ConcurrentDictionary<DependencyRegistration, object> regs))
          store[CTX_INSTANCES_KEY_CONCURRENT] = regs = new ConcurrentDictionary<DependencyRegistration, object>();
        return regs;
      }
    }
  }
}