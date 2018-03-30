using System;
using System.Collections.Generic;
using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.DI;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public abstract class registration_depending_on_enum_of_unregistered_after_registration: dependency_resolver_context
  {
    [Test]
    public void func_is_resolved()
    {
      var factory = Resolver.Resolve<Func<IEnumerable<Simple>>>();
      Resolver.AddDependency<Simple>();
      factory().ShouldHaveSingleItem().ShouldNotBeNull();
    }
  }
}