using OpenRasta.DI.Internal;

namespace OpenRasta.DI
{
  public abstract class DependencyLifetimeManager
  {
    public abstract object Resolve(ResolveContext context, DependencyRegistration registration);

    public virtual void EndScope()
    {
    }
  }
}