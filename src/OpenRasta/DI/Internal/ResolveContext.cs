using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  public class ResolveContext
  {
    readonly Stack<DependencyRegistration> _recursionDefender = new Stack<DependencyRegistration>();

    public ResolveContext(IDependencyRegistrationCollection registrations)
    {
      Registrations = registrations;
    }

    private IDependencyRegistrationCollection Registrations { get; }

    public bool TryResolve(Type serviceType, out object instance)
    {
      instance = null;
      var profile = TryGetProfile(serviceType, this);

      return profile != null && profile.TryResolve(out instance);
    }

    public bool TryResolve(DependencyRegistration registration, out object instance)
    {
      instance = null;
      if (_recursionDefender.Contains(registration))
        return false;
      try
      {
        _recursionDefender.Push(registration);
        instance = registration.ResolveInContext(this);
        return true;
      }
      finally
      {
        _recursionDefender.Pop();
      }
    }

    private static ResolveProfile TryGetSimpleProfile(ResolveContext ctx, Type serviceType)
    {
      var registration = ctx.Registrations.DefaultRegistrationFor(serviceType);
      return registration != null ? new SimpleProfile(registration, ctx) : null;
    }

    private static ResolveProfile TryGetEnumProfile(ResolveContext ctx, Type serviceType)
    {
      
      if (!serviceType.IsGenericType ||
          serviceType.IsGenericTypeDefinition ||
          serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
        return null;

      var innerServiceType = serviceType.GetGenericArguments()[0];

      var registrations = ctx.Registrations[innerServiceType];
      
      return (ResolveProfile) Activator
        .CreateInstance(
          typeof(EnumerableProfile<>)
            .MakeGenericType(innerServiceType),
          registrations, ctx);
    }

    private static ResolveProfile TryGetFuncProfile(ResolveContext ctx, Type serviceType)
    {
      if (serviceType.IsGenericType == false
          || serviceType.GetGenericTypeDefinition() != typeof(Func<>))
        return null;
      
      var innerType = serviceType.GetGenericArguments()[0];
      var innerProfile = TryGetProfile(innerType, ctx);
      return innerProfile == null
        ? null
        : (ResolveProfile)Activator.CreateInstance(typeof(FuncProfile<>).MakeGenericType(innerType), innerProfile);
    }

    private static ResolveProfile TryGetProfile(Type serviceType, ResolveContext resolveContext)
    {
      return TryGetSimpleProfile(resolveContext, serviceType)
             ?? TryGetEnumProfile(resolveContext, serviceType)
             ?? TryGetFuncProfile(resolveContext, serviceType);
    }
  }
}