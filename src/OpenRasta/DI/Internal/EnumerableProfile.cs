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
      var serviceTypeRegistrations = _ctx.Registrations[typeof(T)];
      
      if (!serviceTypeRegistrations.Any())
      {
        instance = Enumerable.Empty<T>();
        return true;
      }

      instance = (
        from dependency in serviceTypeRegistrations  
        select (T)_ctx.Resolve(dependency)
      ).ToList();
      
      return true;
    }
  }
}