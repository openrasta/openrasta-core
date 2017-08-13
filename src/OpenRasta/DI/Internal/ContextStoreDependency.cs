using System;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class ContextStoreDependency
  {
    public ContextStoreDependency(string key, object instance, IContextStoreDependencyCleaner cleaner)
    {
      Key = key ?? throw new ArgumentNullException(nameof(key));
      _instance = instance ?? throw new ArgumentNullException(nameof(instance));
      _cleaner = cleaner;
    }

    private IContextStoreDependencyCleaner _cleaner;
    private object _instance;
    public string Key { get; }

    public void Cleanup()
    {
      _cleaner?.Destruct(Key,_instance);
    }
  }
}