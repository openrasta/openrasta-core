using System;
using OpenRasta.Configuration;
using OpenRasta.DI;
using Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Katana
{
  using MidFunc = Func<AppFunc, AppFunc>;
  using AspNetBuildFunc = Action<Func<AppFunc, AppFunc>>;

  public static class AspNetAppFuncExtensions
  {
    public static AspNetBuildFunc UseOpenRasta(this AspNetBuildFunc builder, IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null)
    {
      builder(new OpenRastaMiddleware(configurationSource, dependencyResolver).ToMidFunc());
      return builder;
    }
  }

  public static class AppBuilderExtensions
  {
    public static IAppBuilder UseOpenRasta(this IAppBuilder builder, IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null)
    {
      return builder.Use(new OpenRastaMiddleware(configurationSource, dependencyResolver).ToMidFunc());
    }
  }
}