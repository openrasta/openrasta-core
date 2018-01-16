using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.DI;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Katana
{
  using MidFunc = Func<AppFunc, AppFunc>;
  public static class AspNetCoreBuildFuncExtensions
  {
    public static Action<MidFunc> UseOpenRasta(
      this Action<MidFunc> builder,
      IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null,
      CancellationToken onAppDisposing = default(CancellationToken))
    {
      builder(new OpenRastaMiddleware(configurationSource, dependencyResolver, onAppDisposing).ToMidFunc());
      return builder;
    }
  }

  public static class OwinDelegates
  {
    public static MidFunc CreateMiddleware(IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null,
      CancellationToken onAppDisposing = default(CancellationToken))
    {
      return new OpenRastaMiddleware(configurationSource, dependencyResolver, onAppDisposing).ToMidFunc();
    }
  }
}