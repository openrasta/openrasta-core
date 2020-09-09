using System;
using System.Threading;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.DI;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Owin
{
  using MidFunc = Func<AppFunc, AppFunc>;
  public static class AspNetCoreBuildFuncExtensions
  {
    public static Action<MidFunc> UseOpenRasta(this Action<MidFunc> builder,
      IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null,
      CancellationToken onAppDisposing = default, StartupProperties startupProperties = null)
    {
      builder(new OpenRastaMiddleware(configurationSource, dependencyResolver, onAppDisposing, startupProperties).ToMidFunc());
      return builder;
    }
  }

  public static class OwinDelegates
  {
    public static MidFunc CreateMiddleware(IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null,
      CancellationToken onAppDisposing = default, StartupProperties startupProperties = null)
    {
      return new OpenRastaMiddleware(configurationSource, dependencyResolver, onAppDisposing, startupProperties).ToMidFunc();
    }
  }
}