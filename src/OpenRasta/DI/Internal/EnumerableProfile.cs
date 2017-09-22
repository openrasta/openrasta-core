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

    public override object TryResolve()
    {
      if (!_ctx.Registrations.HasRegistrationForService(typeof(T)))
        return Enumerable.Empty<T>();
      var resolved = (
        from dependency in _ctx.Registrations[typeof(T)]
        where dependency.IsRegistrationAvailable
        select _ctx.Resolve<T>(dependency)
      ).ToList();
      return resolved;
    }
  }
}