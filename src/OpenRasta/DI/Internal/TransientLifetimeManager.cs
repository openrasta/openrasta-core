namespace OpenRasta.DI.Internal
{
  internal class TransientLifetimeManager : DependencyLifetimeManager
  {
    public TransientLifetimeManager(InternalDependencyResolver builder)
      : base(builder)
    {
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      return registration.CreateInstance(context);
    }
  }
}