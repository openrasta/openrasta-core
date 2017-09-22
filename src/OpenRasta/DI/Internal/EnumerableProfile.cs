using System.Linq;

namespace OpenRasta.DI.Internal
{
  class EnumerableProfile<T> : ResolveProfile
  {
    readonly ResolveContext _ctx;

    public EnumerableProfile(ResolveContext ctx)
    {
      _ctx = ctx;
    }

    public override bool TryResolve(out object instance)
    {
      if (!_ctx.Registrations.HasRegistrationForService(typeof(T)))
      {
        instance = Enumerable.Empty<T>();
        return true;
      }
      instance = (
        from dependency in _ctx.Registrations[typeof(T)]
        where dependency.IsRegistrationAvailable
        select _ctx.Resolve<T>(dependency)
      ).ToList();
      return true;
    }
  }
}