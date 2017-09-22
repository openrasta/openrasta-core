using System;

namespace OpenRasta.DI.Internal
{
  class SimpleProfile : ResolveProfile
  {
    readonly ResolveContext _ctx;
    readonly Type _serviceType;

    public SimpleProfile(ResolveContext ctx, Type serviceType)
    {
      _ctx = ctx;
      _serviceType = serviceType;
    }

    public override bool TryResolve(out object instance)
    {
      return _ctx.Registrations.TryResolve(_ctx, _serviceType, out instance);
    }
  }
}