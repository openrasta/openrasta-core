using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
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

    static HostManager()
    {
      Log = DependencyManager.IsAvailable
        ? DependencyManager.GetService<ILogger>()
        : TraceSourceLogger.Instance;
    }

    HostManager(IHost host)
    {
      Host = host;
      var withStartup = host as IHostStartWithStartupProperties;
      if (withStartup != null)
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
    }

    public IHost Host { get; }
    public bool IsConfigured { get; private set; }

    public IDependencyResolver Resolver { get; private set; }
    static ILogger Log { get; set; }

    public static HostManager RegisterHost(IHost host)
    {
      if (host == null) throw new ArgumentNullException(nameof(host));

      Log.WriteInfo("Registering host of type {0}", host.GetType());

      var manager = new HostManager(host);

      lock (_registrations)
        _registrations.Add(host, manager);
      return manager;
    }

    public static void UnregisterHost(IHost host)
    {
      Log.WriteInfo("Unregistering host of type {0}", host.GetType());
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
      Resolver = Host.ResolverAccessor != null
        ? Host.ResolverAccessor.Resolver
        : new InternalDependencyResolver();
      if (!Resolver.HasDependency<IDependencyResolver>())
        Resolver.AddDependencyInstance(typeof(IDependencyResolver), Resolver);
      Log.WriteDebug("Using dependency resolver of type {0}", Resolver.GetType());
    }

    void Configure(StartupProperties startupProperties)
    {
      IsConfigured = false;
      _startupProperties = startupProperties;
      AssignResolver();
      Resolver.AddDependencyInstance<IHost>(Host, DependencyLifetime.Singleton);
      CallWithDependencyResolver(() =>
      {
        RegisterRootDependencies();

        VerifyContextStoreRegistered();

        RegisterCoreDependencies();

        RegisterLeafDependencies();

        ExecuteConfigurationSource();

        BuildPipeline();

        IsConfigured = true;
      });
    }

    void BuildPipeline()
    {
      _pipeline = Resolver.Resolve<IPipelineInitializer>().Initialize(_startupProperties);

    }

    void ExecuteConfigurationSource()
    {
      if (Resolver.HasDependency<IConfigurationSource>())
      {
        var configSource = Resolver.Resolve<IConfigurationSource>();
        Log.WriteDebug("Using configuration source {0}", configSource.GetType());
        configSource.Configure();
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

    async Task CallWithDependencyResolver(Func<Task> action)
    {
      DependencyManager.SetResolver(Resolver);
      try
      {
        await action();
      }
      finally
      {
        DependencyManager.UnsetResolver();
      }
    }

    void CallWithDependencyResolver(Action action)
    {
      DependencyManager.SetResolver(Resolver);
      try
      {
        action();
      }
      finally
      {
        DependencyManager.UnsetResolver();
      }
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
      var task = CallWithDependencyResolver(async () =>
      {
        // register the required dependency in the web context
        var context = e.Context;
        SetupCommunicationContext(context);

        await _pipeline.RunAsync(context);
      });
      e.Context.PipelineData[Keys.Request.PipelineTask] = e.RunTask = task;
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
      Log.WriteDebug("Request finished.");
      CallWithDependencyResolver(() => Resolver.HandleIncomingRequestProcessed());
    }
  }
}