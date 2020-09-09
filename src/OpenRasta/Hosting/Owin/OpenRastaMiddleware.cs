using System;
using System.Threading;
using System.Threading.Tasks;
using LibOwin;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Web;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OpenRasta.Hosting.Owin
{
  using MidFunc = Func<AppFunc, AppFunc>;

  class OwinMiddleware
  {
    protected OwinMiddleware Next { get; private set; }

    OwinMiddleware Compose(OwinMiddleware next = null)
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
        var composed = Compose(new AppFuncMiddleware(next));
        return env => composed.Invoke(new OwinContext(env));
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

  class OpenRastaMiddleware : OwinMiddleware, IDisposable
  {
    static readonly object SyncRoot = new object();
    HostManager _hostManager;
    readonly OwinHost _host;

    public OpenRastaMiddleware(IConfigurationSource options,
      IDependencyResolverAccessor resolverAccessor = null,
      CancellationToken onDisposing = default, StartupProperties startupProperties = null)
    {
      _host = new OwinHost(options, resolverAccessor);
      TryInitializeHosting(onDisposing, startupProperties ?? DefaultStartupProperties);
    }

    static readonly StartupProperties DefaultStartupProperties = new StartupProperties();

    public override async Task Invoke(IOwinContext owinContext)
    {
      ICommunicationContext commContext;
      owinContext.Response.OnSendingHeaders(_ => this.HeadersSent = true, null);
      try
      {
        commContext = await _host.ProcessRequestAsync(owinContext);
      }
      catch (Exception e) when (HeadersSent == false)
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

    public bool HeadersSent { get; set; }


    void TryInitializeHosting(CancellationToken onDisposing, StartupProperties startup)
    {
      if (_hostManager != null) return;
      lock (SyncRoot)
      {
        Thread.MemoryBarrier();
        if (_hostManager != null) return;

        var hostManager = HostManager.RegisterHost(_host);

        onDisposing.Register(Dispose);

        Thread.MemoryBarrier();
        _hostManager = hostManager;
        try
        {
          _host.RaiseStart(startup);
        }
        catch
        {
          HostManager.UnregisterHost(_host);
          _hostManager = null;
          throw;
        }
      }
    }

    public void Dispose()
    {
      _host.RaiseStop();

      HostManager.UnregisterHost(_host);
    }
  }
}