using System;

namespace OpenRasta.DI.Internal
{
  internal class FuncProfile<T> : ResolveProfile
  {
    readonly ResolveContext _ctx;
    readonly ResolveProfile _profile;

    public FuncProfile(ResolveContext ctx, ResolveProfile profile)
    {
      _ctx = ctx;
      _profile = profile;
    }

    public override object TryResolve()
    {
      return ResolveTyped();
    }

    Func<T> ResolveTyped()
    {
      return () => (T) _profile.TryResolve();
    }
  }
}