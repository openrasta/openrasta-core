using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.InMemory
{
  public class InMemoryHost : IHost, IHostStartWithStartupProperties, IDependencyResolverAccessor, IDisposable
  {
    readonly IConfigurationSource _configuration;
    bool _isDisposed;
    string _applicationVirtualPath;

    public InMemoryHost() :
      this((IConfigurationSource) null)
    {
    }

    public InMemoryHost(Action configuration, IDependencyResolver dependencyResolver = null,
      StartupProperties startup = null)
      : this(new DelegateConfigurationSource(configuration), dependencyResolver, startup)
    {
    }

    public InMemoryHost(
      IConfigurationSource configuration,
      IDependencyResolver dependencyResolver = null,
      StartupProperties startup = null)
    {
      Resolver = dependencyResolver ?? new InternalDependencyResolver();
      _configuration = configuration;
      ApplicationVirtualPath = "/";
      HostManager = HostManager.RegisterHost(this);
      RaiseStart(startup ?? new StartupProperties());
    }

    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

    public event EventHandler Stop;

    public X509Certificate2 ClientCertificate { get; set; } = null;

    public string ApplicationVirtualPath
    {
      get => _applicationVirtualPath;
      set => _applicationVirtualPath = value.EndsWith("/") ? value : value + "/";
    }

    public HostManager HostManager { get; }
    public IDependencyResolver Resolver { get; }

    IDependencyResolverAccessor IHost.ResolverAccessor => this;

    public void Close()
    {
      RaiseStop(new StartupProperties());
      HostManager.UnregisterHost(this);
      _isDisposed = true;
    }

    [Obsolete("Please use the async version, this one may and will deadlock")]
    public IResponse ProcessRequest(IRequest request)
    {
      return ProcessRequestAsync(request).Result;
    }

    public async Task<IResponse> ProcessRequestAsync(IRequest request)
    {
      CheckNotDisposed();
      var ambientContext = new AmbientContext();
      var context = new InMemoryCommunicationContext
      {
        ApplicationBaseUri = new Uri(
          new Uri("http://localhost/", UriKind.Absolute),
          new Uri(ApplicationVirtualPath, UriKind.Relative)),
        Request = request,
        Response = new InMemoryResponse(),
        ServerErrors = new ServerErrorList {Log = Resolver.Resolve<ILogger>()},
      };
      context.PipelineData.Owin.SslLoadClientCertAsync = () =>
      {
        context.PipelineData.Owin.SslClientCertificate = ClientCertificate;
        return Task.CompletedTask;
      };
      try
      {
        using (new ContextScope(ambientContext))
        {
          await RaiseIncomingRequestReceived(context);
        }
      }
      finally
      {
        using (new ContextScope(ambientContext))
        {
          RaiseIncomingRequestProcessed(context);
        }
      }

      if (context.Response.Entity?.Stream.CanSeek == true)
        context.Response.Entity.Stream.Position = 0;
      return context.Response;
    }

    public Task LoadCertAsync { get; set; }

    void IDisposable.Dispose()
    {
      Close();
    }

    bool IHost.ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      CheckNotDisposed();
      return true;
    }

    bool IHost.ConfigureRootDependencies(IDependencyResolver resolver)
    {
      CheckNotDisposed();
      resolver.AddDependencyInstance<IContextStore>(new InMemoryContextStore());
      if (_configuration != null)
        Resolver.AddDependencyInstance(_configuration);
      return true;
    }

    protected virtual void RaiseIncomingRequestProcessed(ICommunicationContext context)
    {
      IncomingRequestProcessed.Raise(this, new IncomingRequestProcessedEventArgs(context));
    }

    protected virtual Task RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      var incomingRequestReceivedEventArgs = new IncomingRequestReceivedEventArgs(context);
      IncomingRequestReceived.Raise(this, incomingRequestReceivedEventArgs);
      return incomingRequestReceivedEventArgs.RunTask;
    }

    event EventHandler _legacyStart;
    event EventHandler<StartupProperties> _start;

    event EventHandler IHost.Start
    {
      add => _legacyStart += value;
      remove => _legacyStart -= value;
    }

    event EventHandler<StartupProperties> IHostStartWithStartupProperties.Start
    {
      add => _start += value;
      remove => _start -= value;
    }

    void RaiseStart(StartupProperties properties)
    {
      _legacyStart.Raise(this);
      var start = _start;
      start?.Invoke(this, properties);
    }

    void RaiseStop(StartupProperties startupProperties)
    {
      Stop.Raise(this, EventArgs.Empty);
    }

    void CheckNotDisposed()
    {
      if (_isDisposed)
        throw new ObjectDisposedException("HttpListenerHost");
    }

    class DelegateConfigurationSource : IConfigurationSource
    {
      readonly Action _configuration;

      public DelegateConfigurationSource(Action configuration)
      {
        _configuration = configuration;
      }

      public void Configure()
      {
        using (OpenRastaConfiguration.Manual)
        {
          _configuration();
        }
      }
    }
  }
}