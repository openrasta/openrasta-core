using System;
using System.Threading.Tasks;
using LibOwin;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  public class OwinHost : IHost, IHostStartWithStartupProperties
  {
    public OwinHost(
      IConfigurationSource configuration,
      IDependencyResolverAccessor resolverAccesor = null,
      string applicationVirtualPath = "/")
    {
      ConfigurationSource = configuration;
      ResolverAccessor = resolverAccesor ?? configuration as IDependencyResolverAccessor;
      ApplicationVirtualPath = applicationVirtualPath;
    }

    public IConfigurationSource ConfigurationSource { get; set; }

    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;

    public bool ConfigureRootDependencies(IDependencyResolver resolver)
    {
      if (ConfigurationSource != null)
        resolver.AddDependencyInstance(ConfigurationSource);
      resolver.AddDependency<IContextStore, AmbientContextStore>(DependencyLifetime.Singleton);
      resolver.AddDependency<ILogger<OwinLogSource>, TraceSourceLogger<OwinLogSource>>(
        DependencyLifetime.Transient);
      return true;
    }

    public bool ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      return true;
    }

    public string ApplicationVirtualPath { get; }
    public IDependencyResolverAccessor ResolverAccessor { get; }
    event EventHandler LegacyStart;
    event EventHandler IHost.Start
    {
      add => LegacyStart += value;
      remove => LegacyStart -= value;
    }

    public event EventHandler Stop;
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

    internal async Task<ICommunicationContext> ProcessRequestAsync(IOwinContext owinContext)
    {
      var commContext = new OwinCommunicationContext(owinContext, TraceSourceLogger.Instance);
      
      var ambientContext = new AmbientContext();

      try
      {
        using (new ContextScope(ambientContext))
        {
          await RaiseIncomingRequestReceived(commContext);
        }
      }
      finally
      {
        using (new ContextScope(ambientContext))
        {
          RaiseIncomingRequestProcessed(commContext);
        }
      }
      return commContext;
    }

    internal virtual Task RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      var request = new IncomingRequestReceivedEventArgs(context);
      IncomingRequestReceived.Raise(this, request);
      return request.RunTask;
    }

    internal void RaiseIncomingRequestProcessed(ICommunicationContext context)
    {
      IncomingRequestProcessed.Raise(this, new IncomingRequestProcessedEventArgs(context));
    }

    internal virtual void RaiseStart()
    {
      RaiseStart(new StartupProperties());
    }

    public void RaiseStop()
    {
      Stop?.Raise(this);
    }

    public event EventHandler<StartupProperties> Start;

    internal virtual void RaiseStart(StartupProperties e)
    {
      LegacyStart?.Invoke(this, EventArgs.Empty);
      Start?.Invoke(this, e);
    }
  }
}