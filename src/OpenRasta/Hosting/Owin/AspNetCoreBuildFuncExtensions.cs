using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.DI;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Katana
{
  using MidFunc = Func<AppFunc, AppFunc>;
  public static class AspNetCoreBuildFuncExtensions
  {
    public static Action<MidFunc> UseOpenRasta(this Action<MidFunc> builder, IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null)
    {
      builder(new OpenRastaMiddleware(configurationSource, dependencyResolver).ToMidFunc());
      return builder;
    }
  }

  public static class OwinDelegates
  {
    public static MidFunc CreateMiddleware(IConfigurationSource configurationSource,
      IDependencyResolverAccessor dependencyResolver = null)
    {
      return new OpenRastaMiddleware(configurationSource, dependencyResolver).ToMidFunc();
    }
  }
}