using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
    public class AspNetHost : IHost
    {
        private readonly object _syncRoot = new object();
        public AspNetHost()
        {
        }

        public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;
        public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

        public event EventHandler Start;
        public event EventHandler Stop;

        public string ApplicationVirtualPath
        {
            get { return HttpRuntime.AppDomainAppVirtualPath; }
        }

        private IConfigurationSource _configurationSource;
        private IDependencyResolverAccessor _resolver;

        public IConfigurationSource ConfigurationSource
        {
            get
            {
                if (_configurationSource == null)
                    lock(_syncRoot)
                    {
                        if (_configurationSource == null)
                        {
                            var source = FindTypeInProject<IConfigurationSource>();
                            Thread.MemoryBarrier();
                            _configurationSource = source;
                        }
                    }
                return _configurationSource;
            }
            set
            {
                _configurationSource = value;
            }
        }

        public IDependencyResolverAccessor ResolverAccessor
        {
            get
            {
                if (_resolver == null)
                    lock(_syncRoot)
                    {
                        if (_resolver == null)
                        {
                            var resolver = ConfigurationSource as IDependencyResolverAccessor ?? FindTypeInProject<IDependencyResolverAccessor>();
                            Thread.MemoryBarrier();
                            _resolver = resolver;
                        }
                    }
                return _resolver;
            }
        }

        public static T FindTypeInProject<T>() where T : class
        {
            // forces global.asax to be compiled.
            BuildManager.GetReferencedAssemblies();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(NotFrameworkAssembly))
            {
                try
                {
                    using(RedirectToLoadedAssembliesToShutUpPolicyRedirection())
                    {
                        var configType = assembly.GetTypes()
                            .FirstOrDefault(t => typeof(T).IsAssignableFrom(t));
                        if (configType != null && configType.IsClass)
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
                if (!_disposed)
                {
                    _disposed = true;
                    _onDispose();
                }
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
            return true;
        }

        public bool ConfigureRootDependencies(IDependencyResolver resolver)
        {
            resolver.AddDependency<IContextStore, AspNetContextStore>(DependencyLifetime.Singleton);
            resolver.AddDependency<OpenRastaRewriterHandler>(DependencyLifetime.Transient);
            resolver.AddDependency<OpenRastaIntegratedHandler>(DependencyLifetime.Transient);
            resolver.AddDependency<ILogger<AspNetLogSource>, TraceSourceLogger<AspNetLogSource>>(DependencyLifetime.Transient);
            return true;
        }

        protected internal void RaiseIncomingRequestProcessed(ICommunicationContext context)
        {
            IncomingRequestProcessed.Raise(this, new IncomingRequestProcessedEventArgs(context));
        }

        protected internal virtual void RaiseIncomingRequestReceived(ICommunicationContext context)
        {
            IncomingRequestReceived.Raise(this, new IncomingRequestReceivedEventArgs(context));
        }

        protected internal virtual void RaiseStart()
        {
            Start.Raise(this);
        }

        protected internal virtual void RaiseStop()
        {
            Stop.Raise(this);
            _resolver = null;
        }
    }
}