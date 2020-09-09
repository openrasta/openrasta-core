using System.Threading;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.Owin;
using Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Katana
{
  public static class AppBuilderExtensions
  {
    public static IAppBuilder UseOpenRasta(this IAppBuilder builder, IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null, StartupProperties properties = null)
    {
      var onAppDisposing =
        builder.Properties.TryGetValue("host.OnAppDisposing", out var val) &&
        val is CancellationToken owinDispose
          ? owinDispose
          : CancellationToken.None;
      return builder.Use(OwinDelegates.CreateMiddleware(configurationSource, dependencyResolver, onAppDisposing, properties));
    }
  }
}