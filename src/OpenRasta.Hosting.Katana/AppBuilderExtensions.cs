using OpenRasta.Configuration;
using OpenRasta.DI;
using Owin;

namespace OpenRasta.Hosting.Katana
{
  public static class AppBuilderExtensions
  {
    public static IAppBuilder UseOpenRasta(this IAppBuilder builder, IConfigurationSource configurationSource)
    {
      return builder.Use(typeof(OpenRastaMiddleware), configurationSource);
    }

    public static IAppBuilder UseOpenRasta(this IAppBuilder builder, IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver)
    {
      return builder.Use(typeof(OpenRastaMiddleware), configurationSource, dependencyResolver);
    }
  }
}