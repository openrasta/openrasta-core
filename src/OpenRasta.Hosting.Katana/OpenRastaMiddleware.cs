using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;

namespace OpenRasta.Hosting.Katana
{
  public class OpenRastaMiddleware : OwinMiddleware
  {
    static readonly object SyncRoot = new object();
    HostManager _hostManager;

    public OpenRastaMiddleware(OwinMiddleware next, IConfigurationSource options)
      : base(next)
    {
      Host = new OwinHost(options);
    }

    public OpenRastaMiddleware(OwinMiddleware next, IConfigurationSource options,
      IDependencyResolverAccessor resolverAccesor)
      : base(next)
    {
      Host = new OwinHost(options, resolverAccesor);
    }

    static ILogger<OwinLogSource> Log { get; set; }
    static OwinHost Host { get; set; }

    public override async Task Invoke(IOwinContext owinContext)
    {
      TryInitializeHosting();

      try
      {
        owinContext = ProcessRequest(owinContext);
      }
      catch (Exception e)
      {
        owinContext.Response.StatusCode = 500;
        owinContext.Response.Write(e.ToString());
      }

      await Next.Invoke(owinContext);
    }

    IOwinContext ProcessRequest(IOwinContext owinContext)
    {
      var context = new OwinCommunicationContext(owinContext, Log);

      lock (SyncRoot)
      {
        Host.RaiseIncomingRequestReceived(context);

        Host.RaiseIncomingRequestProcessed(context);
      }

      return owinContext;
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