using System;
using System.Collections.Generic;
using System.Net;
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

    async Task ProcessContext()
    {
      QueueNextRequestPending();
      HttpListenerContext nativeContext;

      try
      {
        nativeContext = await _listener.GetContextAsync();
      }
      catch (HttpListenerException)
      {
        return;
      }
      var ambientContext = new AmbientContext();
      var context = new HttpListenerCommunicationContext(this, nativeContext, Resolver.ResolveWithDefault(() => TraceSourceLogger.Instance));
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

    public void StartListening()
    {
      CheckNotDisposed();
      using (new ContextScope(new AmbientContext()))
      {
        Start(this, EventArgs.Empty);
      }
      _listener.Start();
      QueueNextRequestPending();
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
    }

    void CheckNotDisposed()
    {
      if (_isDisposed)
        throw new ObjectDisposedException("HttpListenerHost");
    }

    void QueueNextRequestPending()
    {
      Task.Run(ProcessContext);
    }
  }
}