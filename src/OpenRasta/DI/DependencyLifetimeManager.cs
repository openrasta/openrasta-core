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

    public virtual bool IsRegistrationAvailable(DependencyRegistration registration)
    {
      return true;
    }

    public abstract object Resolve(ResolveContext context, DependencyRegistration registration);

    protected static object CreateObject(ResolveContext context, DependencyRegistration registration)
    {
      return context.Builder.CreateObject(registration);
    }

    public virtual void VerifyRegistration(DependencyRegistration registration)
    {
    }
  }
}