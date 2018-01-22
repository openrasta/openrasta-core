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

    public override bool TryResolve(IDependencyRegistrationCollection registrations, ResolveContext resolveContext, out object instance)
    {
      instance = ResolveTyped(registrations, resolveContext);
      return true;
    }

    Func<T> ResolveTyped(IDependencyRegistrationCollection registrations, ResolveContext resolveContext) =>
      () => _innerProfile(registrations, resolveContext, out var instance)
        ? (T) instance
        : throw new DependencyResolutionException($"Could not resolve type {typeof(T)}");
  }
}