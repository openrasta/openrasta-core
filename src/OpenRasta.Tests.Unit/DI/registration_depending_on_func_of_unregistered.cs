using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using Shouldly;
using InternalDependencyResolver_Specification;

namespace OpenRasta.Tests.Unit.DI
{
  public abstract class registration_depending_on_func_of_unregistered : dependency_resolver_context
  {
    [Test]
    public void can_resolve_type()
    {
      Resolver.AddDependency<DependsOnFuncOfSimple>();
      Should.NotThrow(() => Resolver.Resolve<DependsOnFuncOfSimple>());
    }

    [Test]
    public virtual void cannot_resolve_func_before_type_is_registered()
    {
      Resolver.AddDependency<DependsOnFuncOfSimple>();

      var dependent = Resolver.Resolve<DependsOnFuncOfSimple>();
      Should.Throw<DependencyResolutionException>(() =>
      {
        var simple = dependent.Simple();
        simple.ShouldNotBeNull();
      });
    }
  }

  public class DependsOnFuncOfSimple
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