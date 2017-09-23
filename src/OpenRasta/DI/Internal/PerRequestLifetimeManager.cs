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
      if (!_resolver.TryResolve<IContextStore>(out var store))
        throw new DependencyResolutionException(
          "Could not resolve the context store. Make sure you have registered one.");
      
      return store
        .GetConcurrentContextInstances()
        .GetOrAdd(registration, reg => reg.Factory(context));
    }

    public override void EndScope()
    {
      _resolver.Resolve<IContextStore>().GetConcurrentContextInstances().Clear();
    }
  }
}