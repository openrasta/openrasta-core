using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI.Internal;
using OpenRasta.Pipeline;

namespace OpenRasta.DI
{
  public class InternalDependencyResolver : DependencyResolverCore, IDependencyResolver, IRequestScopedResolver
  {
    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimeManagers;
    private readonly GlobalRegistrations _registrations;
    private const string CTX_REGISTRATIONS = "openrasta.di.requestRegistrations";

    private IDependencyRegistrationCollection Registrations => 
      new ResolveContext(_registrations).TryResolve<IContextStore>(out var ctx) &&
      ctx.TryGet<IDependencyRegistrationCollection>(CTX_REGISTRATIONS, out var ctxRegistrations)
          ? ctxRegistrations
          : _registrations;

    public InternalDependencyResolver()
    {
      _registrations = new GlobalRegistrations();
      _lifetimeManagers = new Dictionary<DependencyLifetime, DependencyLifetimeManager>
      {
        {DependencyLifetime.Transient, new TransientLifetimeManager()},
        {DependencyLifetime.Singleton, new SingletonLifetimeManager()},
        {DependencyLifetime.PerRequest, new PerRequestLifetimeManager(this)}
      };
    }

    protected override void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime)
    {
      Registrations.Add(new DependencyRegistration(serviceType, concreteType, lifetime,_lifetimeManagers[lifetime]));
    }

    protected override void AddDependencyCore(Type concreteType, DependencyLifetime lifetime)
    {
      AddDependencyCore(concreteType, concreteType, lifetime);
    }

    protected override void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime)
    {
      var registrations = Registrations;
      
      if (lifetime == DependencyLifetime.PerRequest && registrations == _registrations)
        throw new InvalidOperationException("Request scope is not available, cannot register PerRequest instances");
      
      registrations.Add(new DependencyRegistration(
        serviceType,
        instance.GetType(),
        lifetime,
        _lifetimeManagers[lifetime],
        context=>instance));
    }

    protected override IEnumerable<TService> ResolveAllCore<TService>()
    {
      return ((IEnumerable<object>) ResolveCore(typeof(IEnumerable<TService>))).Cast<TService>();
    }


    public bool TryResolve<T>(out T instance)
    {
      var success = new ResolveContext(Registrations).TryResolve(typeof(T), out var untyped);
      instance = (T)untyped;
      return success;
    }
    protected override object ResolveCore(Type serviceType)
    {
      try
      {
        return new ResolveContext(Registrations).Resolve(serviceType);
      }
      catch (Exception e)
      {
        throw new DependencyResolutionException($"Could not resolve dependencies for {serviceType}", e);
      }
    }

    [Obsolete]
    public void HandleIncomingRequestProcessed()
    {
      throw new NotSupportedException();
    }

    public bool HasDependency(Type serviceType)
    {
      return serviceType != null && Registrations.HasRegistrationForService(serviceType);
    }

    public bool HasDependencyImplementation(Type serviceType, Type concreteType)
    {
      return Registrations.HasRegistrationForService(serviceType) &&
             Registrations[serviceType].Count(r => r.ConcreteType == concreteType) >= 1;
    }

    public IDisposable CreateRequestScope()
    {
      var requestContextRegistrations = new RequestContextRegistrations(_registrations);
      var contextStore = new ResolveContext(_registrations)
        .Resolve<IContextStore>();
      contextStore
        .Add(CTX_REGISTRATIONS, requestContextRegistrations);
      return new ActionOnDispose(() =>
      {
        _lifetimeManagers[DependencyLifetime.PerRequest].EndScope();
        requestContextRegistrations.Dispose();
        contextStore.Remove(CTX_REGISTRATIONS);
      });
    }
  }
}