namespace OpenRasta.DI.Internal
{
  internal class TransientLifetimeManager : DependencyLifetimeManager
  {
    public TransientLifetimeManager(InternalDependencyResolver builder)
      : base(builder)
    {
    }

    public override bool Contains(DependencyRegistration registration)
    {
      return true;
    }

    public override object Resolve(ResolveContext context, DependencyRegistration registration)
    {
      return registration.CreateInstance(context);
    }

    public override void ClearScope()
    {
      
    }
  }
}