using OpenRasta.DI.Internal;

namespace OpenRasta.DI
{
  public abstract class DependencyLifetimeManager
  {
    protected DependencyLifetimeManager(InternalDependencyResolver resolver)
    {
      Resolver = resolver;
    }

    protected InternalDependencyResolver Resolver { get; }

    public virtual bool Contains(DependencyRegistration registration)
    {
      return registration.LifetimeManager == this;
    }

    public abstract object Resolve(ResolveContext context, DependencyRegistration registration);

    public virtual void Add(DependencyRegistration registration)
    {
    }

    public virtual void ClearScope()
    {
    }
  }
}