using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetHost : IHost, IHostStartWithStartupProperties
  {
    readonly StartupProperties _properties;
    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

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

    protected internal virtual void RaiseStart()
    {
      _legacyStart.Raise(this);
      var start = _start;
      start?.Invoke(this, _properties);
    }

    public event EventHandler Stop;

    public string ApplicationVirtualPath => HttpRuntime.AppDomainAppVirtualPath;

    IConfigurationSource _configurationSource;

    Lazy<IConfigurationSource> _configSourceFactory;
    readonly Lazy<IDependencyResolverAccessor> _resolverAccessor;


    public AspNetHost(StartupProperties properties)
    {
      _properties = properties;
      _resolverAccessor = new Lazy<IDependencyResolverAccessor>(CreateResolverAccessor);
      _configSourceFactory = new Lazy<IConfigurationSource>(ConfigurationSourceLocator);
    }

    public IConfigurationSource ConfigurationSource
    {
      get { return _configurationSource ?? _configSourceFactory.Value; }
      set { _configurationSource = value; }
    }

    public IDependencyResolverAccessor ResolverAccessor => _resolverAccessor.Value;

    IDependencyResolverAccessor CreateResolverAccessor()
    {
      return (ConfigurationSource as IDependencyResolverAccessor)
             ?? DependencyResolverAccessorLocator();
    }

    public static Func<IConfigurationSource> ConfigurationSourceLocator = FindTypeInProject<IConfigurationSource>;
    public static Func<IDependencyResolverAccessor> DependencyResolverAccessorLocator = FindTypeInProject<IDependencyResolverAccessor>;

    public static T FindTypeInProject<T>() where T : class
    {
      // forces global.asax to be compiled.
      BuildManager.GetReferencedAssemblies();
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(NotFrameworkAssembly))
      {
        try
        {
          using (RedirectToLoadedAssembliesToShutUpPolicyRedirection())
          {
            var configType = assembly.GetTypes()
              .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && t.IsClass);
            if (configType != null)
            {
              return Activator.CreateInstance(configType) as T;
            }
          }
        }
        catch
        {
        }
      }
      return null;
    }

    static IDisposable RedirectToLoadedAssembliesToShutUpPolicyRedirection()
    {
      AppDomain.CurrentDomain.AssemblyResolve += ResolveToAlreadyLoaded;
      return new ActionOnDispose(() => AppDomain.CurrentDomain.AssemblyResolve -= ResolveToAlreadyLoaded);
    }

    class ActionOnDispose : IDisposable
    {
      readonly Action _onDispose;

      bool _disposed;

      public ActionOnDispose(Action onDispose)
      {
        _onDispose = onDispose;
      }

      public void Dispose()
      {
        if (_disposed) return;
        _disposed = true;
        _onDispose();
      }
    }

    static Assembly ResolveToAlreadyLoaded(object sender, ResolveEventArgs args)
    {
      return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_ => _.FullName == args.Name);
    }

    public static bool NotFrameworkAssembly(Assembly assembly)
    {
      switch (assembly.GetName().Name)
      {
        case "System":
        case "mscorlib":
        case "System.Core":
        case "System.Configuration":
        case "System.Data":
        case "System.Web":
        case "System.Xml":
        case "OpenRasta":
        case "OpenRasta.Hosting.AspNet":
          return false;
        default:
          return true;
      }
    }

    public bool ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      if (ConfigurationSource != null)
        resolver.AddDependencyInstance<IConfigurationSource>(ConfigurationSource);
      var uris = resolver.Resolve<IUriResolver>();
      var config = resolver.Resolve<IMetaModelRepository>();
      return true;
    }

    public bool ConfigureRootDependencies(IDependencyResolver resolver)
    {
      resolver.AddDependency<IContextStore, AspNetContextStore>(DependencyLifetime.Singleton);
      resolver.AddDependency<ILogger<AspNetLogSource>, TraceSourceLogger<AspNetLogSource>>(DependencyLifetime
        .Transient);
      return true;
    }

    protected internal void RaiseIncomingRequestProcessed(ICommunicationContext context)
    {
      IncomingRequestProcessed.Raise(this, new IncomingRequestProcessedEventArgs(context));
    }

    protected internal virtual Task RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      var incomingRequestReceivedEventArgs = new IncomingRequestReceivedEventArgs(context);
      IncomingRequestReceived.Raise(this, incomingRequestReceivedEventArgs);
      return incomingRequestReceivedEventArgs.RunTask;
    }


    protected internal virtual void RaiseStop()
    {
      Stop.Raise(this);
    }

  }
}