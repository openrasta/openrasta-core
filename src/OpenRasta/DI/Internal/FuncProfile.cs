using System;

namespace OpenRasta.DI.Internal
{
  class FuncProfile<T> : ResolveProfile
  {
    readonly ProfileResolver _innerProfile;

    public FuncProfile(ProfileResolver innerProfile)
    {
      _innerProfile = innerProfile;
    }

    public override bool TryResolve(out object instance)
    {
      instance = ResolveTyped();
      return true;
    }

    Func<T> ResolveTyped() =>
      () => _innerProfile(out var instance)
        ? (T) instance
        : throw new DependencyResolutionException($"Could not resolve type {typeof(T)}");
  }
}