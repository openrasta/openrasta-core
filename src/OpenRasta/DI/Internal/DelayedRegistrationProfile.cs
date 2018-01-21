using System;

namespace OpenRasta.DI.Internal
{
  class DelayedRegistrationProfile : ResolveProfile
  {
    readonly Func<DependencyRegistration> _tryFindRegistration;
    readonly ResolveContext _ctx;

    public DelayedRegistrationProfile(Func<DependencyRegistration> tryFindRegistration, ResolveContext ctx)
    {
      _tryFindRegistration = tryFindRegistration;
      _ctx = ctx;
    }

    public override bool TryResolve(out object instance)
    {
      var reg = _tryFindRegistration();
      if (reg != null) return _ctx.TryResolve(reg, out instance);

      instance = null;
      return false;
    }
  }
}