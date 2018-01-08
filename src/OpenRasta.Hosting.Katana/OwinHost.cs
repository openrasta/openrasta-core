using System;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.Katana
{
  public class OwinHost : IHost
  {
    public OwinHost(IConfigurationSource configuration)
    {
      ConfigurationSource = configuration;
    }

    public OwinHost(IConfigurationSource configuration, IDependencyResolverAccessor resolverAccesor)
    {
      ConfigurationSource = configuration;
      ResolverAccessor = resolverAccesor;
    }

    public IConfigurationSource ConfigurationSource { get; set; }

    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;

    public bool ConfigureRootDependencies(IDependencyResolver resolver)
    {
      resolver.AddDependency<IContextStore, OwinContextStore>(DependencyLifetime.Singleton);
      resolver.AddDependency<ICommunicationContext, OwinCommunicationContext>(DependencyLifetime.PerRequest);
      resolver.AddDependency<ILogger<OwinLogSource>, TraceSourceLogger<OwinLogSource>>(
        DependencyLifetime.Transient);
      return true;
    }

    public bool ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      if (ConfigurationSource != null)
        resolver.AddDependencyInstance(ConfigurationSource);
      return true;
    }

    public string ApplicationVirtualPath { get; private set; }
    public IDependencyResolverAccessor ResolverAccessor { get; }
    public event EventHandler Start;
    public event EventHandler Stop;
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

    internal virtual void RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      IncomingRequestReceived.Raise(this, new IncomingRequestReceivedEventArgs(context));
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