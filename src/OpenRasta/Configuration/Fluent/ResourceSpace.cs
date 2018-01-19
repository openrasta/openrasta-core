using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Internal;

// ReSharper disable once CheckNamespace
namespace OpenRasta.Configuration
{
  public static class ResourceSpace
  {
    /// <summary>
    /// Registers resources
    /// </summary>
    public static IHas Has => AsyncLocalConfigurations.Target;

    /// <summary>
    /// Register services and modules
    /// </summary>
    public static IUses Uses => AsyncLocalConfigurations.Target;
  }
}