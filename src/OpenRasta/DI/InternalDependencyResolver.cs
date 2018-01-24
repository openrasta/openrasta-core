using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI.Internal;
using OpenRasta.Hosting;
using OpenRasta.Pipeline;

namespace OpenRasta.DI
{
  public class InternalDependencyResolver :
    DependencyResolverCore,
    IDependencyResolver,
    IRequestScopedResolver,
    IModelDrivenDependencyRegistration
  {
    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimeManagers;
    readonly GlobalRegistrations _globalRegistrations;
    const string CTX_REGISTRATIONS = "openrasta.di.requestRegistrations";


    IContextStore _cached;
    bool _changesSinceStoreLookup = true;

    IContextStore ContextStore
    {
      get
      {
        if (_cached != null) return _cached;
        if (!_changesSinceStoreLookup) return null;

        if (new ResolveContext(() => _globalRegistrations).TryResolve(out _cached))
          return _cached;

        _changesSinceStoreLookup = false;
        return null;
      }
    }

    IDependencyRegistrationCollection Registrations
    {
      get
      {
        try
        {
          var ctxRegistrations = ContextStore?[CTX_REGISTRATIONS] as IDependencyRegistrationCollection;
          return ctxRegistrations ?? _globalRegistrations;
        }
        catch
        {
          return _globalRegistrations;
        }
      }
    }

    public InternalDependencyResolver()
    {
      _globalRegistrations = new GlobalRegistrations();
      _lifetimeManagers = new Dictionary<DependencyLifetime, DependencyLifetimeManager>
      {
        {DependencyLifetime.Transient, new TransientLifetimeManager()},
        {DependencyLifetime.Singleton, new SingletonLifetimeManager()},
        {DependencyLifetime.PerRequest, new PerRequestLifetimeManager(this)}
      };
      AddDependencyInstance(typeof(IDependencyResolver), this, DependencyLifetime.Singleton);
      AddDependencyInstance(typeof(IModelDrivenDependencyRegistration), this, DependencyLifetime.Singleton);
    }

    protected override void AddDependencyCore(Type concreteType, DependencyLifetime lifetime)
    {
      AddDependencyCore(concreteType, concreteType, lifetime);
    }

    protected override void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime)
    {
      Registrations.Add(new DependencyRegistration(serviceType, concreteType, lifetime, _lifetimeManagers[lifetime]));
      _changesSinceStoreLookup = true;
    }


    protected override void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime)
    {
      if (lifetime == DependencyLifetime.PerRequest && Registrations == _globalRegistrations)
        throw new InvalidOperationException("Request scope is not available, cannot register PerRequest instances");

      Registrations.Add(new DependencyRegistration(
        serviceType,
        instance.GetType(),
        lifetime,
        _lifetimeManagers[lifetime],
        context => instance));
      _changesSinceStoreLookup = true;
    }

    protected override IEnumerable<TService> ResolveAllCore<TService>()
    {
      return ((IEnumerable<object>) ResolveCore(typeof(IEnumerable<TService>))).Cast<TService>();
    }


    public bool TryResolve<T>(out T instance)
    {
      var success = new ResolveContext(() => Registrations).TryResolve(typeof(T), out var untyped);
      instance = (T) untyped;
      return success;
    }

    protected override object ResolveCore(Type serviceType)
    {
      try
      {
        return new ResolveContext(() => Registrations).Resolve(serviceType);
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
             Registrations[serviceType].Any(r => r.ConcreteType == concreteType);
    }

    public IDisposable CreateRequestScope()
    {
      if (ContextStore == null)
        throw new DependencyResolutionException("Cannot resolve per-request outisde of request scope.");
      var requestContextRegistrations = new RequestContextRegistrations(_globalRegistrations);
      ContextStore[CTX_REGISTRATIONS] = requestContextRegistrations;

      return new ActionOnDispose(() =>
      {
        _lifetimeManagers[DependencyLifetime.PerRequest].EndScope();
        requestContextRegistrations.Dispose();
        ContextStore[CTX_REGISTRATIONS] = null;
      });
    }

    public void Register(DependencyFactoryModel registration)
    {
      object resolveFromRegistration(ResolveContext ctx)
      {
        return registration.Invoker(registration.Arguments.Select(ctx.Resolve).ToArray());
      }

      Func<ResolveContext, object> factory = null;
      if (registration.Factory != null)
        factory = resolveFromRegistration;
      
      Registrations.Add(new DependencyRegistration(
        registration.ServiceType,
        registration.ConcreteType,
        registration.Lifetime,
        _lifetimeManagers[registration.Lifetime],
        factory
      ));
    }
  }
}