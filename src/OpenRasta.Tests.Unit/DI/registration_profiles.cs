using System;
using System.Collections.Generic;
using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.TypeSystem;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public abstract class registration_profiles : dependency_resolver_context
  {
    [Test]
    public void resolving_funcs()
    {
      Resolver.AddDependency<Simple>();

      Should.NotThrow(() =>
      {
        var factory = Resolver.Resolve<Func<Simple>>();
        return factory();
      });
    }
    

    [Test]
    public void resolving_enums()
    {
      Should.NotThrow(() => Resolver.Resolve<IEnumerable<Simple>>().ShouldHaveSingleItem().ShouldNotBeNull());
    }
    [Test]
    public void resolving_func_of_enums()
    {
      Resolver.AddDependency<Simple>();
      Should.NotThrow(() => Resolver.Resolve<Func<IEnumerable<Simple>>>()().ShouldHaveSingleItem().ShouldNotBeNull());
    }
    
    [Test, Ignore("This won't work yet, needs refactoring")]
    public void resolving_enum_of_func()
    {
      Resolver.AddDependency<Simple>();
      Should.NotThrow(() =>
      {
        var enumerable = Resolver.Resolve<IEnumerable<Func<Simple>>>();
        var func = enumerable.ShouldHaveSingleItem();
        func();
      });
    }
  }
}