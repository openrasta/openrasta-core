using System.Collections.Generic;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Tests.Unit.DI;
using Shouldly;

namespace InternalDependencyResolver_Specification
{
  public abstract class when_resolving_instances : dependency_resolver_context
  {
    public class TypeWithDependencyResolverAsProperty
    {
      public IDependencyResolver Resolver { get; set; }
    }

    public class TypeWithPropertyAlreadySet
    {
      public TypeWithPropertyAlreadySet()
      {
        Resolver = new InternalDependencyResolver();
      }

      public IDependencyResolver Resolver { get; set; }
    }

    [Test]
    public void a_property_that_would_cause_a_cyclic_dependency_is_ignored()
    {
      Resolver.AddDependency<RecursiveProperty>();

      Resolver.Resolve<RecursiveProperty>().Property.ShouldBeNull();
    }

    [Test]
    public void a_type_cannot_be_created_when_its_dependencies_are_not_registered()
    {
      Resolver.AddDependency<IAnother, Another>();

      Executing(() => Resolver.Resolve<IAnother>()).ShouldThrow<DependencyResolutionException>();
    }

    [Test]
    public void an_empty_enumeration_of_unregistered_types_is_resolved()
    {
      var simpleList = Resolver.ResolveAll<ISimple>();

      simpleList.ShouldNotBeNull();
      simpleList.ShouldBeEmpty();
    }

    [Test]
    public void resolves_enumerables()
    {
      Resolver.AddDependency<ISimple, Simple>();
      Resolver.AddDependency<ISimple, AnotherSimple>();
      var result = Resolver.Resolve<IEnumerable<ISimple>>();
      result.ShouldContain(o => o is Simple);
      result.ShouldContain(o => o is AnotherSimple);
    }

    [Test]
    public void resolves_dependent_enumerables()
    {
      Resolver.AddDependency<IDependent<IEnumerable<ISimple>>, Dependent<IEnumerable<ISimple>>>();
      Resolver.AddDependency<ISimple, Simple>();

      var instance = Resolver.Resolve<IDependent<IEnumerable<ISimple>>>();
      var deps = instance.CtorDependencies();
      deps.ShouldContain(d => d is Simple);
    }

    [Test]
    public void a_type_can_get_a_dependency_resolver_dependency_assigned()
    {
      Resolver.AddDependencyInstance(typeof(IDependencyResolver), Resolver);
      Resolver.AddDependency<TypeWithDependencyResolverAsProperty>(DependencyLifetime.Transient);

      Resolver.Resolve<TypeWithDependencyResolverAsProperty>()
        .Resolver.ShouldBeSameAs(Resolver);
    }

    [Test]
    public void a_property_for_which_there_is_a_property_already_assigned_is_replaced_with_value_from_container()
    {
      Resolver.AddDependencyInstance(typeof(IDependencyResolver), Resolver);
      Resolver.AddDependency<TypeWithPropertyAlreadySet>(DependencyLifetime.Singleton);

      Resolver.Resolve<TypeWithPropertyAlreadySet>()
        .Resolver.ShouldBeSameAs(Resolver);
    }
  }
}