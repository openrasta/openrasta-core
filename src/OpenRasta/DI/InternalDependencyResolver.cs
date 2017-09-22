using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Concordia;
using OpenRasta.DI.Internal;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.DI
{
  public class InternalDependencyResolver : DependencyResolverCore, IDependencyResolver, IRequestScopedResolver
  {
    readonly Dictionary<DependencyLifetime, DependencyLifetimeManager> _lifetimeManagers;
    private readonly DependencyRegistrationCollection _registrations;
    private const string CTX_REGISTRATIONS = "openrasta.di.requestRegistrations";

    public IDependencyRegistrationCollection Registrations => 
      new ResolveContext(_registrations).TryResolve<IContextStore>(out var ctx) &&
      ctx.TryGet<IDependencyRegistrationCollection>(CTX_REGISTRATIONS, out var ctxRegistrations)
          ? ctxRegistrations
          : _registrations;

    public InternalDependencyResolver()
    {
      _registrations = new DependencyRegistrationCollection();
      _lifetimeManagers = new Dictionary<DependencyLifetime, DependencyLifetimeManager>
      {
        {DependencyLifetime.Transient, new TransientLifetimeManager()},
        {DependencyLifetime.Singleton, new SingletonLifetimeManager()},
        {DependencyLifetime.PerRequest, new PerRequestLifetimeManager(this)}
      };
    }

    protected override void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime)
    {
      Registrations.Add(new DependencyRegistration(serviceType, concreteType, _lifetimeManagers[lifetime]));
    }

    protected override void AddDependencyCore(Type concreteType, DependencyLifetime lifetime)
    {
      AddDependencyCore(concreteType, concreteType, lifetime);
    }

    protected override void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime)
    {
      Registrations.Add(new DependencyRegistration(
        serviceType,
        instance.GetType(),
        _lifetimeManagers[lifetime],
        instance));
    }

    protected override IEnumerable<TService> ResolveAllCore<TService>()
    {
      return ((IEnumerable<object>) ResolveCore(typeof(IEnumerable<TService>))).Cast<TService>();
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
      new ResolveContext(_registrations)
        .Resolve<IContextStore>()
        .Add(CTX_REGISTRATIONS, new RequestContextRegistrations(_registrations));
      return new ActionOnDispose(() =>
        _lifetimeManagers[DependencyLifetime.PerRequest].ClearScope());
    }
  }
}