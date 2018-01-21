using System;

namespace OpenRasta.DI.Internal
{
  public static class ResolveContextExtensions
  {
    public static object Resolve(this ResolveContext context, Type serviceType)
    {
      return context.TryResolve(serviceType, out var instance)
        ? instance
        : throw new DependencyResolutionException($"Could not find a resolve profile for {serviceType}");
    }
    
    public static bool TryResolve<T>(this ResolveContext ctx, out T instance)
    {
      instance = default;
      var success = ctx.TryResolve(typeof(T), out var untyped);
      if (success) instance = (T) untyped;
      return success;
    }

    public static object Resolve(this ResolveContext ctx, DependencyRegistration registration)
    {
      return ctx.TryResolve(registration, out var instance)
        ? instance
        : throw new InvalidOperationException("Recursive dependencies are not allowed.");
    }

  }
}