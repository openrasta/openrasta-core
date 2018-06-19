using System;
using Castle.Core;

namespace OpenRasta.DI.Windsor
{
  public static class ConvertLifestyles
  {
    public static LifestyleType ToLifestyleType(DependencyLifetime lifetime)
    {
      switch (lifetime)
      {
        case DependencyLifetime.Singleton:
          return LifestyleType.Singleton;
        case DependencyLifetime.PerRequest:
          return LifestyleType.Scoped;
                case DependencyLifetime.Transient:
                    return LifestyleType.Transient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "The provided lifetime is not recognized.");
            }
        }
    }
}