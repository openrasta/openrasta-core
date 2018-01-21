using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Pipeline;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public class registration_depending_on_func_in_other_scope
  {
    [Test]
    public void resolve_succeeds()
    {
      var resolver = new InternalDependencyResolver();
      resolver.AddDependency<IContextStore, AmbientContextStore>();
      
      resolver.AddDependency<Contributor>(DependencyLifetime.Singleton);
      resolver.AddDependency<ErrorCollector>(DependencyLifetime.Transient);

      var contributor = resolver.Resolve<Contributor>();
      using(new ContextScope(new AmbientContext()))
      using (resolver.CreateRequestScope())
      {
        resolver.AddDependencyInstance(new Request(), DependencyLifetime.PerRequest);
        var result = contributor.Factory();
        result.ShouldHaveSingleItem();
      }
    }
    class Request{}

    class ErrorCollector
    {
      public ErrorCollector(Request request)
      {
      }
    }

    class Contributor
    {
      public Func<IEnumerable<ErrorCollector>> Factory { get; }

      public Contributor(Func<IEnumerable<ErrorCollector>> factory)
      {
        Factory = factory;
      }
    }
  }

  public class registration_depending_on_func_of_unregistered
  {
    readonly InternalDependencyResolver resolver;

    public registration_depending_on_func_of_unregistered()
    {
      resolver = new InternalDependencyResolver();
      resolver.AddDependency<DependsOnFuncOfSimple>();
    }

    [Test]
    public void can_resolve_type()
    {
      Should.NotThrow(() => resolver.Resolve<DependsOnFuncOfSimple>());
    }

    [Test]
    public void cannot_resolve_func_before_type_is_registered()
    {
      var dependent = resolver.Resolve<DependsOnFuncOfSimple>();
      Should.Throw<DependencyResolutionException>(() =>
      {
        var simple = dependent.Simple();
        simple.ShouldNotBeNull();
      });
    }
  }

  class DependsOnFuncOfSimple
  {
    public DependsOnFuncOfSimple(Func<Simple> simple)
    {
      Simple = simple;
    }

    public Func<Simple> Simple { get; }
  }

  class DependsOnFuncOfEnumOfSimple
  {
    public DependsOnFuncOfEnumOfSimple(Func<IEnumerable<Simple>> simple)
    {
      Simple = simple;
    }

    public Func<IEnumerable<Simple>> Simple { get; }
  }
}