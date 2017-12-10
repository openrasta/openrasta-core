using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.HttpListener
{
  public class HttpListenerHost : MarshalByRefObject, IHost, IDisposable
  {
    readonly IConfigurationSource _configuration;
    bool _isDisposed;
    System.Net.HttpListener _listener;
    IDependencyResolverAccessor _resolverAccessor;
    Type _resolverFactory;
    int _pendingRequestCount;
    readonly ManualResetEvent _zeroPendingRequests = new ManualResetEvent(true);

    ~HttpListenerHost()
    {
      Dispose(false);
    }

    public HttpListenerHost() { }

    public HttpListenerHost(IConfigurationSource configuration)
    {
      _configuration = configuration;
    }

    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed = (s, e) => { };
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived = (s, e) => { };

    public event EventHandler Start = (s, e) => { };
    public event EventHandler Stop = (s, e) => { };
    public string ApplicationVirtualPath { get; private set; }
    public IDependencyResolver Resolver { get; private set; }

    public IDependencyResolverAccessor ResolverAccessor
    {
      get
      {
        if (_resolverFactory != null && _resolverAccessor == null)
        {
          _resolverAccessor = (IDependencyResolverAccessor) Activator.CreateInstance(_resolverFactory);
        }

        return _resolverAccessor;
      }
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Initialize(IEnumerable<string> prefixes, string appPathVDir, Type dependencyResolverFactory)
    {
      CheckNotDisposed();
      
      ApplicationVirtualPath = appPathVDir;

      _resolverFactory = dependencyResolverFactory;
      _listener = new System.Net.HttpListener();
      foreach (string prefix in prefixes)
        _listener.Prefixes.Add(prefix);
      HostManager.RegisterHost(this);
    }

    public override object InitializeLifetimeService()
    {
      return null;
    }

    async Task AcceptRequests()
    {
      while (_listener.IsListening)
      {
        try
        {
          var context = await _listener.GetContextAsync();
#pragma warning disable 4014
          Task.Run(async () => await ProcessContext(context));
#pragma warning restore 4014
        }
        catch (HttpListenerException)
        {
          return;
        }
      }
    }

    async Task ProcessContext(HttpListenerContext nativeContext)
    {
      var ambientContext = new AmbientContext();
      var context = new HttpListenerCommunicationContext(this, nativeContext, Resolver.Resolve<ILogger>());
      try
      {
        Interlocked.Increment(ref _pendingRequestCount);
        _zeroPendingRequests.Reset();

        try
        {
          using (new ContextScope(ambientContext))
          {
            var incomingRequestReceivedEventArgs = new IncomingRequestReceivedEventArgs(context);
            IncomingRequestReceived(this, incomingRequestReceivedEventArgs);
            await incomingRequestReceivedEventArgs.RunTask;
          }
        }
        finally
        {
          using (new ContextScope(ambientContext))
          {
            IncomingRequestProcessed(this, new IncomingRequestProcessedEventArgs(context));
          }
        }
      }
      finally
      {
        if (Interlocked.Decrement(ref _pendingRequestCount) == 0)
        {
          _zeroPendingRequests.Set();
        }
      }
    }

    public void StartListening()
    {
      CheckNotDisposed();
      using (new ContextScope(new AmbientContext()))
      {
        Start(this, EventArgs.Empty);
      }
      _listener.Start();
      Task.Run(AcceptRequests);
    }

    public void StopListening()
    {
      CheckNotDisposed();

      using (new ContextScope(new AmbientContext()))
      {
        Stop(this, EventArgs.Empty);
      }
      _listener.Stop();
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public virtual bool ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      return true;
    }

    public virtual bool ConfigureRootDependencies(IDependencyResolver resolver)
    {
      Resolver = resolver;
      resolver.AddDependency<IContextStore, AmbientContextStore>(DependencyLifetime.Singleton);
      if (_configuration != null) resolver.AddDependencyInstance(_configuration);

      return true;
    }

    protected virtual void Dispose(bool fromDisposeMethod)
    {
      if (_isDisposed || _listener == null) return;
      
      if (fromDisposeMethod)
      {
        if (_listener.IsListening)
          StopListening();
        HostManager.UnregisterHost(this);
      }
      _listener.Abort();
      _listener.Close();
      _isDisposed = true;

      _zeroPendingRequests.WaitOne(TimeSpan.FromSeconds(5));
    }

    void CheckNotDisposed()
    {
      if (_isDisposed)
        throw new ObjectDisposedException("HttpListenerHost");
    }
  }
}
