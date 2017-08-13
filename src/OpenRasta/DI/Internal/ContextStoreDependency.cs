using System;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class ContextStoreDependency
  {
    public ContextStoreDependency(DependencyRegistration registration, object instance, IContextStoreDependencyCleaner cleaner)
    {
      _instance = instance ?? throw new ArgumentNullException(nameof(instance));
      _registration = registration;
      _cleaner = cleaner;
    }

    private readonly DependencyRegistration _registration;
    private readonly IContextStoreDependencyCleaner _cleaner;
    private readonly object _instance;
    public string Key => _registration.Key;

    public void Cleanup()
    {
      _cleaner?.Destruct(_registration,_instance);
    }
  }
}