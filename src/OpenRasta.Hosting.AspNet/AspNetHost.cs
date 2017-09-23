using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
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

    protected internal void RaiseStart()
    {
      _legacyStart.Raise(this);
      var start = _start;
      start?.Invoke(this, _properties);
    }

    public event EventHandler Stop;

    public string ApplicationVirtualPath => HttpRuntime.AppDomainAppVirtualPath;

    readonly Lazy<IConfigurationSource> _configSourceFactory;
    readonly Lazy<IDependencyResolverAccessor> _resolverAccessor;


    public AspNetHost(StartupProperties properties)
    {
      _properties = properties;
      _resolverAccessor = new Lazy<IDependencyResolverAccessor>(CreateResolverAccessor);
      _configSourceFactory = new Lazy<IConfigurationSource>(ConfigurationSourceLocator);
    }

    IConfigurationSource ConfigurationSource => _configSourceFactory.Value;

    public IDependencyResolverAccessor ResolverAccessor => _resolverAccessor.Value;

    IDependencyResolverAccessor CreateResolverAccessor()
    {
      // ReSharper disable once SuspiciousTypeConversion.Global - Declared API without test
      return (ConfigurationSource as IDependencyResolverAccessor)
             ?? DependencyResolverAccessorLocator();
    }

    public static Func<IConfigurationSource> ConfigurationSourceLocator =
      FindTypeInOpenRastaProject<IConfigurationSource>;

    static readonly Func<IDependencyResolverAccessor> DependencyResolverAccessorLocator =
      FindTypeInOpenRastaProject<IDependencyResolverAccessor>;

    public static T FindTypeInOpenRastaProject<T>() where T : class
    {
      ForceAspNetGlobalAsaxCompilation();

      var localAssemblies =
        Path.GetDirectoryName(new Uri(typeof(AspNetHost).Assembly.EscapedCodeBase).LocalPath);
      var asms = Directory.GetFiles(localAssemblies, "*.dll", SearchOption.TopDirectoryOnly);

      Type[] loadTypes(Assembly assembly)
      {
        try
        {
          return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
          return e.Types.Where(t => t != null).ToArray();
        }
      }

      var potentialTypes =
      (
        from asmPath in asms
        let roAsm = Assembly.ReflectionOnlyLoadFrom(asmPath)
        where roAsm.GetReferencedAssemblies().Any(name => name.Name.EqualsOrdinalIgnoreCase("openrasta"))
        where NotFrameworkAssembly(roAsm)
        let asm = Assembly.Load(roAsm.GetName())
        from configType in loadTypes(asm)
        where configType.IsClass &&
              configType.IsAbstract == false &&
              typeof(T).IsAssignableFrom(configType)
        select configType
      ).ToList();

      if (potentialTypes.Any() == false) return null;
      if (potentialTypes.Count > 1)
        throw new InvalidOperationException($"Looking for {typeof(T)} but found more than one.{Environment.NewLine}" +
                                            string.Join(Environment.NewLine,
                                              potentialTypes.Select(t => t.AssemblyQualifiedName)));
      return (T) Activator.CreateInstance(potentialTypes[0]);
    }

    private static void ForceAspNetGlobalAsaxCompilation()
    {
      BuildManager.GetReferencedAssemblies();
    }

    static readonly byte[] msKey = {0xb, 0x7, 0x7, 0xa, 0x5, 0xc, 0x5, 0x6, 0x1, 0x9, 0x3, 0x4, 0xe, 0x0, 0x8, 0x9};

    static bool NotFrameworkAssembly(Assembly assembly)
    {
      var assemblyName = assembly.GetName();
      return assemblyName.Name != "OpenRasta" &&
             assemblyName.Name != "OpenRasta.Hosting.AspNet" &&
             !assemblyName.GetPublicKeyToken().SequenceEqual(msKey);
    }

    public bool ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      if (ConfigurationSource != null)
        resolver.AddDependencyInstance<IConfigurationSource>(ConfigurationSource);
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

    protected internal Task RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      var incomingRequestReceivedEventArgs = new IncomingRequestReceivedEventArgs(context);
      IncomingRequestReceived.Raise(this, incomingRequestReceivedEventArgs);
      return incomingRequestReceivedEventArgs.RunTask;
    }

    protected internal void RaiseStop()
    {
      Stop.Raise(this);
    }
  }
}