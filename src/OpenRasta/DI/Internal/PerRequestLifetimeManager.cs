using System;
using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class PerRequestLifetimeManager : DependencyLifetimeManager
  {
    private readonly InternalDependencyResolver _resolver;
    

    public PerRequestLifetimeManager(InternalDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      CheckContextStoreAvailable();

      var store = _resolver.Resolve<IContextStore>();

      return store
        .GetConcurrentContextInstances()
        .GetOrAdd(registration, reg => reg.Factory(context));
    }

//    public override void Add(DependencyRegistration registration)
//    {
//      if (!registration.IsInstanceRegistration) return;
//      _resolver.Resolve<IContextStore>().GetConcurrentContextInstances()
//        .TryAdd(registration, registration.CreateInstance(new ResolveContext(_resolver.Registrations)));
//    }

    public override void ClearScope()
    {
      _resolver.Resolve<IContextStore>().GetConcurrentContextInstances().Clear();
    }

    void CheckContextStoreAvailable()
    {
      if (!_resolver.HasDependency(typeof(IContextStore)))
      {
        throw new DependencyResolutionException(
          "Could not resolve the context store. Make sure you have registered one.");
      }
    }
  }
}