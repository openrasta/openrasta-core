using System;

namespace OpenRasta.DI.Internal
{
  internal class FuncProfile<T> : ResolveProfile
  {
    readonly ResolveProfile _innerProfile;

    public FuncProfile(ResolveProfile innerProfile)
    {
      _innerProfile = innerProfile;
    }

    public override bool TryResolve(out object instance)
    {
      instance = ResolveTyped();
      return true;
    }

    // ReSharper disable once MergeConditionalExpression
    Func<T> ResolveTyped()
    {
      return () => _innerProfile.TryResolve(out var instance)
        ? (instance is T ? (T) instance : default(T))
        : default(T);
    }
  }
}