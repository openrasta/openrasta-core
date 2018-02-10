using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting
{
  public class HostManager : IDisposable
  {
    static readonly IDictionary<IHost, HostManager> _registrations = new Dictionary<IHost, HostManager>();
    readonly object _syncRoot = new object();
    readonly Action _startDisposer;
    StartupProperties _startupProperties;
    IPipelineAsync _pipeline;

    HostManager(IHost host)
    {
      Host = host;
      AssignResolver();
      if (host is IHostStartWithStartupProperties withStartup)
      {
        withStartup.Start += HandleHostStartWithProps;
        _startDisposer = () => withStartup.Start -= HandleHostStartWithProps;
      }
      else
      {
        Host.Start += HandleHostStart;
        _startDisposer = () => Host.Start -= HandleHostStart;
      }

      Host.IncomingRequestReceived += HandleHostIncomingRequestReceived;
      Host.IncomingRequestProcessed += HandleIncomingRequestProcessed;
      Host.Stop += HandleHostStop;
    }

    void HandleHostStop(object sender, EventArgs e)
    {
      Dispose();
    }

    public IHost Host { get; }
    public bool IsConfigured { get; private set; }

    public IDependencyResolver Resolver { get; private set; }
    ILogger Log { get; set; }

    public static HostManager RegisterHost(IHost host)
    {
      if (host == null) throw new ArgumentNullException(nameof(host));

      var manager = new HostManager(host);

      lock (_registrations)
        _registrations.Add(host, manager);
      return manager;
    }

    public static void UnregisterHost(IHost host)
    {
      HostManager managerToDispose = null;
      lock (_registrations)
      {
        if (_registrations.ContainsKey(host))
        {
          managerToDispose = _registrations[host];
          _registrations.Remove(host);
        }
      }

      managerToDispose?.Dispose();
    }

    public void SetupCommunicationContext(ICommunicationContext context)
    {
      Log.WriteDebug("Adding communication context data");
      Resolver.AddDependencyInstance<ICommunicationContext>(context, DependencyLifetime.PerRequest);
      Resolver.AddDependencyInstance<IRequest>(context.Request, DependencyLifetime.PerRequest);
      Resolver.AddDependencyInstance<IResponse>(context.Response, DependencyLifetime.PerRequest);
    }

    public void Dispose()
    {
      _startDisposer();
      Host.IncomingRequestReceived -= HandleHostIncomingRequestReceived;
      Host.IncomingRequestProcessed -= HandleIncomingRequestProcessed;

      (Resolver as IDisposable)?.Dispose();
    }

    void AssignResolver()
    {
      Resolver = Host.ResolverAccessor?.Resolver ?? new InternalDependencyResolver();
      Log = Resolver.Resolve<IEnumerable<ILogger>>().LastOrDefault() ?? TraceSourceLogger.Instance;

      Log.WriteDebug("Using dependency resolver of type {0}", Resolver.GetType());
    }

    void Configure(StartupProperties startupProperties)
    {
      IsConfigured = false;
      _startupProperties = startupProperties;

      Resolver.AddDependencyInstance(Host);
      using (DependencyManager.ScopedResolver(Resolver))
      {
        RegisterRootDependencies();

        VerifyContextStoreRegistered();

        RegisterCoreDependencies();

        RegisterLeafDependencies();

        ExecuteConfigurationSource();

        BuildPipeline();

        IsConfigured = true;
      }
    }

    void BuildPipeline()
    {
      _pipeline = Resolver.Resolve<IPipelineInitializer>().Initialize(_startupProperties);
    }

    void ExecuteConfigurationSource()
    {
      if (Resolver.HasDependency<IConfigurationSource>())
      {
        var configurationSource = Resolver.Resolve<IConfigurationSource>();
        Log.WriteDebug("Using configuration source {0}", configurationSource.GetType());

        var configurer = new ConfigurationSourceAdapter(
            configurationSource,
            Resolver,
            Resolver.Resolve<IMetaModelRepository>());

        configurer.Process();
      }
      else
      {
        Log.WriteDebug("Not using any configuration source.");
      }
    }

    void RegisterCoreDependencies()
    {
      var registrar =
          Resolver.ResolveWithDefault<IDependencyRegistrar>(() => new DefaultDependencyRegistrar());
      Log.WriteInfo("Using dependency registrar of type {0}.", registrar.GetType());
      registrar.Register(Resolver);
    }

    void RegisterLeafDependencies()
    {
      Log.WriteDebug("Registering host's leaf dependencies.");
      if (!Host.ConfigureLeafDependencies(Resolver))
        throw new OpenRastaConfigurationException("Leaf dependencies configuration by host has failed.");
    }

    void RegisterRootDependencies()
    {
      Log.WriteDebug("Registering host's root dependencies.");
      if (!Host.ConfigureRootDependencies(Resolver))
        throw new OpenRastaConfigurationException("Root dependencies configuration by host has failed.");
    }

    void VerifyConfiguration(StartupProperties startupProperties)
    {
      if (!IsConfigured)
        lock (_syncRoot)
          if (!IsConfigured)
            Configure(startupProperties);
    }

    void VerifyContextStoreRegistered()
    {
      if (!Resolver.HasDependency<IContextStore>())
        throw new OpenRastaConfigurationException("The host didn't register a context store.");
    }

    protected virtual void HandleHostIncomingRequestReceived(object sender, IncomingRequestEventArgs e)
    {
      Log.WriteDebug("Incoming host request for " + e.Context.Request.Uri);
      e.Context.PipelineData[Keys.Request.PipelineTask] = e.RunTask = HandleHostIncompingRequestReceivedAsync(e);
    }

    async Task HandleHostIncompingRequestReceivedAsync(IncomingRequestEventArgs e)
    {
      using (DependencyManager.ScopedResolver(Resolver))
      {
        e.Context.PipelineData[Keys.Request.ResolverRequestScope] = Resolver.CreateRequestScope();

        // register the required dependency in the web context
        var context = e.Context;
        SetupCommunicationContext(context);

        await _pipeline.RunAsync(context);
      }
    }

    void HandleHostStartWithProps(object sender, StartupProperties e)
    {
      VerifyConfiguration(e);
    }

    protected virtual void HandleHostStart(object sender, EventArgs e)
    {
      VerifyConfiguration(new StartupProperties());
    }

    protected virtual void HandleIncomingRequestProcessed(object sender, IncomingRequestProcessedEventArgs e)
    {
      using (DependencyManager.ScopedResolver(Resolver))
        ((IDisposable)e.Context.PipelineData[Keys.Request.ResolverRequestScope]).Dispose();
    }
  }
}