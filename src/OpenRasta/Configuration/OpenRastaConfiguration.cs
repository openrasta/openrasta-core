using System;
using OpenRasta.Configuration.Fluent.Internal;

namespace OpenRasta.Configuration
{
  public static class OpenRastaConfiguration
  {
    /// <summary>
    /// Creates a manual configuration of the resources supported by the application.
    /// </summary>
    [Obsolete("Call to the using block is only to be used for compatibility.")]
    public static IDisposable Manual => new FluentConfigurator();

    class FluentConfigurator : IDisposable
    {
      public void Dispose()
      {
        AsyncLocalConfigurations.ConfigurationCompletion();
        AsyncLocalConfigurations.ConfigurationCompletion = null;
      }
    }
  }
}