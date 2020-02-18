using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using Shouldly;

namespace Tests.DI.SpeedyGonzales
{
  public class SpeedyGonzalesResolver : IEnumerable
  {
    public List<DependencyFactoryModel> Registrations { get; } = new List<DependencyFactoryModel>();

    readonly ScopeCache _singletons = new ScopeCache();
    readonly ScopeCache _transient = new ScopeCache();

    public void Add(Action<ITypeRegistrationContext> registration)
    {
      var typeRegistration = new TypeRegistrationContext();
      registration(typeRegistration);
      Registrations.Add(typeRegistration.Model);
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public T Resolve<T>()
    {
      return _singletons.TryGetInstance<T>(out var result)
        ? result
        : _transient.TryGetInstance<T>(out var instanceResult)
          ? instanceResult
          : throw new DependencyResolutionException();
    }

    public void Seal()
    {
      var dependencies = new DependencyGraphBuilder(Registrations).RewrittenNodes;
    
      
      foreach (var reg in dependencies)
      {
        if (reg.FactoryExpression.Parameters.Any()) continue;
        switch (reg.Model.Lifetime)
        {
          case DependencyLifetime.Singleton:
          {
            var instance = reg.Factory();
            _singletons.StoreInstance(reg.Model.ServiceType, ()=> instance);
            break;
          }
          case DependencyLifetime.Transient:
            _transient.StoreInstance(reg.Model.ServiceType, () => reg.Factory());
            break;
        }
      }
    }
  }
}