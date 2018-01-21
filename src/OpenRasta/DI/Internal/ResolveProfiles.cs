using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  static class ResolveProfiles
  {
    public static ProfileResolver Find(
      ResolveContext context,
      DependencyRegistration reg)
    {
      return Simple.Find(context, reg)
             ?? Func.Find(context, reg);
    }

    public static ProfileResolver Find(Type serviceType, ResolveContext context,
      Func<IDependencyRegistrationCollection> registrations, bool delayedParentProfile = false)
    {
      return Simple.Find(serviceType, context, registrations)
             ?? Func.Find(serviceType, context, registrations)
             ?? Enumerable.Find(serviceType, context, registrations)
             ?? (delayedParentProfile
               ? Delayed.Find(serviceType, context, registrations)
               : null);
    }

    static class Simple
    {
      public static ProfileResolver Find(Type serviceType, ResolveContext context,
        Func<IDependencyRegistrationCollection> registrations)
      {
        return Find(context, registrations().DefaultRegistrationFor(serviceType));
      }

      public static ProfileResolver Find(ResolveContext context, DependencyRegistration reg)
      {
        return reg != null ? new SimpleProfile(reg, context).TryResolve : default(ProfileResolver);
      }
    }

    static class Func
    {
      public static ProfileResolver Find(ResolveContext context, DependencyRegistration registration)
      {
        var serviceType = registration.ServiceType;
        if (serviceType.IsGenericType == false
            || serviceType.GetGenericTypeDefinition() != typeof(Func<>))
          return null;

        var innerType = serviceType.GetGenericArguments()[0];
        var innerProfile = ResolveProfiles.Find(context, registration);

        ProfileResolver getTypedResolver(ProfileResolver resolver)
        {
          var profile =
            (ResolveProfile) Activator.CreateInstance(typeof(FuncProfile<>).MakeGenericType(innerType), innerProfile);
          return profile.TryResolve;
        }

        return innerProfile != null ? getTypedResolver(innerProfile) : null;
      }

      public static ProfileResolver Find(Type serviceType, ResolveContext context,
        Func<IDependencyRegistrationCollection> registrations)
      {
        if (serviceType.IsGenericType == false
            || serviceType.GetGenericTypeDefinition() != typeof(Func<>))
          return null;

        var innerType = serviceType.GetGenericArguments()[0];
        var innerProfile = ResolveProfiles.Find(innerType, context, registrations, true);

        ProfileResolver getTypedResolver(ProfileResolver resolver)
        {
          var profile =
            (ResolveProfile) Activator.CreateInstance(typeof(FuncProfile<>).MakeGenericType(innerType), innerProfile);
          return profile.TryResolve;
        }

        return innerProfile != null
          ? getTypedResolver(innerProfile)
          : null;
      }
    }

    static class Delayed
    {
      public static ProfileResolver Find(
        Type serviceType, 
        ResolveContext context,
        Func<IDependencyRegistrationCollection> registrations)
      {
        return (out object instance) =>
        {
          var ctx = new ResolveContext(registrations);
          return ctx.TryResolve(serviceType, out instance);
        };
      }
    }

    static class Enumerable
    {
      public static ProfileResolver Find(
        Type serviceType,
        ResolveContext context,
        Func<IDependencyRegistrationCollection> registrations)
      {
        if (!serviceType.IsGenericType ||
            serviceType.IsGenericTypeDefinition ||
            serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
          return null;

        var innerType = serviceType.GetGenericArguments()[0];


        var profile =
          (ResolveProfile) Activator
            .CreateInstance(typeof(EnumerableProfile<>)
              .MakeGenericType(innerType), context, registrations);
        return profile.TryResolve;
      }
    }
  }
}