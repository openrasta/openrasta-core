using System;
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
}