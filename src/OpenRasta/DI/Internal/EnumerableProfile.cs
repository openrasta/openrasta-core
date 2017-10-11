using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  class EnumerableProfile<T> : ResolveProfile
  {
    private readonly IEnumerable<DependencyRegistration> _registrations;
    private readonly ResolveContext _ctx;

    public EnumerableProfile(IEnumerable<DependencyRegistration> registrations, ResolveContext ctx)
    {
      _registrations = registrations;
      _ctx = ctx;
    }

    public override bool TryResolve(out object instance)
    {
      var serviceTypeRegistrations = _registrations;
      
      if (!serviceTypeRegistrations.Any())
      {
        instance = Enumerable.Empty<T>();
        return true;
      }

      instance = (
        from dependency in serviceTypeRegistrations  
        select (T)_ctx.Resolve(dependency)
      ).ToArray();
      
      return true;
    }
  }
}