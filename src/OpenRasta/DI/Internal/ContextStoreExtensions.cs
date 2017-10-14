using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public static class ContextStoreExtensions
  {
    private const string CTX_INSTANCES_KEY_CONCURRENT = nameof(CTX_INSTANCES_KEY_CONCURRENT);

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