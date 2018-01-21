using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public class registration_profiles
  {
    InternalDependencyResolver resolver;

    public registration_profiles()
    {
      resolver = new InternalDependencyResolver();
      resolver.AddDependency<Simple>();
    }

    [Test]
    public void resolving_funcs()
    {
      Should.NotThrow(() =>
      {
        var factory = resolver.Resolve<Func<Simple>>();
        return factory();
      });
    }
    
    [Test]
    public void resolving_enums()
    {
      Should.NotThrow(() => resolver.Resolve<IEnumerable<Simple>>().ShouldHaveSingleItem().ShouldNotBeNull());
    }

    [Test]
    public void resolving_func_of_enums()
    {
      Should.NotThrow(() => resolver.Resolve<Func<IEnumerable<Simple>>>()().ShouldHaveSingleItem().ShouldNotBeNull());
    }
    
    [Test, Ignore("This won't work yet, needs refactoring")]
    public void resolving_enum_of_func()
    {
      Should.NotThrow(() =>
      {
        var enumerable = resolver.Resolve<IEnumerable<Func<Simple>>>();
        var func = enumerable.ShouldHaveSingleItem();
        func();
      });
    }
  }
}