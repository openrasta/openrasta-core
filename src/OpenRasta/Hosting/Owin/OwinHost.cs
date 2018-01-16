using System;
using System.Threading.Tasks;
using LibOwin;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  public class OwinHost : IHost
  {
    public OwinHost(IConfigurationSource configuration,
      IDependencyResolverAccessor resolverAccesor = null, string applicationVirtualPath = "/")
    {
      ConfigurationSource = configuration;
      ResolverAccessor = resolverAccesor;
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
    public event EventHandler Start;
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
      Start.Raise(this);
    }
  }
}