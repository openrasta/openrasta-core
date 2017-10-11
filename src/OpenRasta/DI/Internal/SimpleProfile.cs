using System;

namespace OpenRasta.DI.Internal
{
  class SimpleProfile : ResolveProfile
  {
    private readonly DependencyRegistration _dependency;
    readonly ResolveContext _ctx;
    readonly Type _serviceType;

    public SimpleProfile(DependencyRegistration dependency, ResolveContext context)
    {
      _dependency = dependency;
      _ctx = context;
    }

    public override bool TryResolve(out object instance)
    {
      return _ctx.TryResolve(_dependency, out instance);
      
    }
  }
}