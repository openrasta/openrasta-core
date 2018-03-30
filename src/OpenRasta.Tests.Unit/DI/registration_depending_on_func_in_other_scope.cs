using System;
using System.Collections.Generic;
using InternalDependencyResolver_Specification;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Pipeline;
using Shouldly;

namespace OpenRasta.Tests.Unit.DI
{
  public abstract class registration_depending_on_func_in_other_scope : dependency_resolver_context
  {
    [Test]
    public void resolve_succeeds()
    {
      Resolver.AddDependency<IContextStore, AmbientContextStore>();
      
      Resolver.AddDependency<Contributor>(DependencyLifetime.Singleton);
      Resolver.AddDependency<ErrorCollector>(DependencyLifetime.Transient);

      var contributor = Resolver.Resolve<Contributor>();
      using(new ContextScope(new AmbientContext()))
      using (Resolver.CreateRequestScope())
      {
        Resolver.AddDependencyInstance(new Request(), DependencyLifetime.PerRequest);
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
}