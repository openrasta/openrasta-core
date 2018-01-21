namespace OpenRasta.DI.Internal
{
  class TransientLifetimeManager : DependencyLifetimeManager
  {
    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      return registration.Factory(context);
    }
  }
}