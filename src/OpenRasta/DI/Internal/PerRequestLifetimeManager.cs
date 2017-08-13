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

      if (!registration.IsInstanceRegistration)
        return true;

      var store = Resolver.Resolve<IContextStore>();

      return store.GetContextInstances().Any(x => x. Key == registration.Key);
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      CheckContextStoreAvailable();

      object instance;
      var contextStore = Resolver.Resolve<IContextStore>();

      if ((instance = contextStore[registration.Key]) != null) return instance;
      
      if (registration.IsInstanceRegistration)
        throw new DependencyResolutionException(
          "A dependency registered as an instance wasn't found. The registration was removed.");

      instance = context.Builder.CreateObject(registration);

      StoreInstanceInContext(contextStore, registration.Key, instance);
      return instance;
    }

    public override void Add(DependencyRegistration registration)
    {
      if (!registration.IsInstanceRegistration) return;
      
      CheckContextStoreAvailable();
      var instance = registration.Instance;
      registration.Instance = null;
      var store = Resolver.Resolve<IContextStore>();
      if (store[registration.Key] != null)
        throw new DependencyResolutionException("An instance is being registered for an existing registration.");

      StoreInstanceInContext(store, registration.Key, instance);
    }

    void CheckContextStoreAvailable()
    {
      if (!Resolver.HasDependency(typeof(IContextStore)))
      {
        throw new DependencyResolutionException(
          "Could not resolve the context store. Make sure you have registered one.");
      }
    }

    void StoreInstanceInContext(IContextStore contextStore, string key, object instance)
    {
      contextStore[key] = instance;
      contextStore.GetContextInstances()
        .Add(new ContextStoreDependency(key, instance, Resolver.Registrations));
    }
  }
}