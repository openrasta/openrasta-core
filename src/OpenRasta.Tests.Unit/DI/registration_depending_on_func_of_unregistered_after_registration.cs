using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  class registration_depending_on_func_of_unregistered_after_registration
  {
    InternalDependencyResolver resolver;
    DependsOnFuncOfSimple instance;

    public registration_depending_on_func_of_unregistered_after_registration()
    {
      resolver = new InternalDependencyResolver();
      resolver.AddDependency<DependsOnFuncOfSimple>();
      instance = resolver.Resolve<DependsOnFuncOfSimple>();
      resolver.AddDependency<Simple>();
    }

    [Test]
    public void func_is_resolved()
    {
      instance.Simple().ShouldNotBeNull();
    }
  }

  class registration_depending_on_enum_of_unregistered_after_registration
  {
    [Test]
    public void func_is_resolved()
    {
      var resolver = new InternalDependencyResolver();
      var factory = resolver.Resolve<Func<IEnumerable<Simple>>>();
      resolver.AddDependency<Simple>();
      factory().ShouldHaveSingleItem().ShouldNotBeNull();
    }
  }
}