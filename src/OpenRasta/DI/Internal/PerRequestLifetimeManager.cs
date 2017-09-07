using System.Linq;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public class PerRequestLifetimeManager : DependencyLifetimeManager
  {
    public PerRequestLifetimeManager(InternalDependencyResolver resolver)
      : base(resolver)
    {
    }

    public override bool Contains(DependencyRegistration registration)
    {
      if (!Resolver.HasDependency(typeof(IContextStore))) return false;

      if (!registration.IsInstanceRegistration) return true;

      var store = Resolver.Resolve<IContextStore>();

      return store.GetConcurrentContextInstances().ContainsKey(registration);
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      CheckContextStoreAvailable();

      var store = Resolver.Resolve<IContextStore>();

      return store.GetConcurrentContextInstances().GetOrAdd(registration, reg => reg.CreateInstance(context));
    }

    public override void Add(DependencyRegistration registration)
    {
      if (!registration.IsInstanceRegistration) return;
      Resolver.Resolve<IContextStore>().GetConcurrentContextInstances()
        .TryAdd(registration, registration.CreateInstance(new ResolveContext(Resolver.Registrations)));
    }

    public override void ClearScope()
    {
      foreach (var transitiveRegistration in Resolver.Resolve<IContextStore>().GetConcurrentContextInstances().Keys)
      {
        if (transitiveRegistration.IsInstanceRegistration)
        {
          Resolver.Registrations.Remove(transitiveRegistration);
        }
      }
    }

    void CheckContextStoreAvailable()
    {
      if (!Resolver.HasDependency(typeof(IContextStore)))
      {
        throw new DependencyResolutionException(
          "Could not resolve the context store. Make sure you have registered one.");
      }
    }
  }
}