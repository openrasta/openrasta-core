namespace OpenRasta.DI.Internal
{
  internal class TransientLifetimeManager : DependencyLifetimeManager
  {
    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      return registration.Factory(context);
    }
  }
}