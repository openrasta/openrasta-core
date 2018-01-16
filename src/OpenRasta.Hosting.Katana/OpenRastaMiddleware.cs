using System;
using System.Threading;
using System.Threading.Tasks;
using LibOwin;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Web;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Katana
{
  using MidFunc = Func<AppFunc, AppFunc>;

  class OwinMiddleware
  {
    protected OwinMiddleware Next { get; set; }

    protected virtual OwinMiddleware Compose(OwinMiddleware next = null)
    {
      Next = next;
      return this;
    }

    public virtual Task Invoke(IOwinContext owinContext)
    {
      return Next?.Invoke(owinContext);
    }

    public MidFunc ToMidFunc()
    {
      return next =>
      {
        Compose(new AppFuncMiddleware(next));
        return env => Invoke(new OwinContext(env));
      };
    }
  }

  class AppFuncMiddleware : OwinMiddleware
  {
    readonly AppFunc _app;

    public AppFuncMiddleware(AppFunc app)
    {
      _app = app;
    }

    public override Task Invoke(IOwinContext owinContext)
    {
      return _app(owinContext.Environment);
    }
  }

  class OpenRastaMiddleware : OwinMiddleware
  {
    static readonly object SyncRoot = new object();
    HostManager _hostManager;

    public OpenRastaMiddleware(IConfigurationSource options,
      IDependencyResolverAccessor resolverAccesor = null)
    {
      Host = new OwinHost(options, resolverAccesor);
    }

    static OwinHost Host { get; set; }

    public override async Task Invoke(IOwinContext owinContext)
    {
      TryInitializeHosting();


      ICommunicationContext commContext;
      try
      {
        commContext = await Host.ProcessRequestAsync(owinContext);
      }
      catch (Exception e)
      {
        owinContext.Response.StatusCode = 500;
        owinContext.Response.Write(e.ToString());
        return;
      }

      if (commContext != null &&
          commContext.OperationResult is OperationResult.NotFound notFound &&
          notFound.Reason == NotFoundReason.NotMapped)
      {
        await Next.Invoke(owinContext);
      }
    }



    void TryInitializeHosting()
    {
      if (_hostManager != null) return;
      lock (SyncRoot)
      {
        Thread.MemoryBarrier();
        if (_hostManager != null) return;

        var hostManager = HostManager.RegisterHost(Host);
        Thread.MemoryBarrier();
        _hostManager = hostManager;
        try
        {
          Host.RaiseStart();
          _hostManager.Resolver.Resolve<ILogger<OwinLogSource>>();
        }
        catch
        {
          HostManager.UnregisterHost(Host);
          _hostManager = null;
          throw;
        }
      }
    }
  }
}