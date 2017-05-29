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

    public override object Resolve()
    {
      if (!_ctx.Registrations.HasRegistrationForService(typeof(T)))
        return System.Linq.Enumerable.Empty<T>();
      var resolved = (
        from dependency in _ctx.Registrations[typeof(T)]
        where dependency.IsRegistrationAvailable(dependency)
        select _ctx.Resolve<T>(dependency)
      ).ToList();
      return resolved;
    }
  }
}