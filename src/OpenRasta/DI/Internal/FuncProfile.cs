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

    public override bool TryResolve(out object instance)
    {
      instance = ResolveTyped();
      return true;
    }

    // ReSharper disable once MergeConditionalExpression
    Func<T> ResolveTyped()
    {
      return () => _profile.TryResolve(out var instance)
        ? (instance is T ? (T) instance : default(T))
        : default(T);
    }
  }
}