using System;
using System.Collections.Generic;

namespace OpenRasta.DI.Internal
{
  abstract class ResolveProfile
  {
    public static ResolveProfile Simple(ResolveContext ctx, Type serviceType)
    {
      return ctx.Registrations.HasRegistrationForService(serviceType)
        ? new SimpleProfile(ctx, serviceType)
        : null;
    }

    public static ResolveProfile Enumerable(ResolveContext ctx, Type serviceType)
    {
      if (serviceType.IsConstructedGenericType == false ||
          serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
        return null;

      var innerServiceType = serviceType.GenericTypeArguments[0];

      return (ResolveProfile) Activator
        .CreateInstance(typeof(EnumerableProfile<>)
          .MakeGenericType(innerServiceType), ctx);
    }

    public abstract object Resolve();
  }
}