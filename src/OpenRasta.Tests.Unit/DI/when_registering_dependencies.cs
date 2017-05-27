using System;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Tests.Unit.DI;
using Shouldly;

namespace InternalDependencyResolver_Specification
{
  public abstract class when_registering_dependencies : dependency_resolver_context
  {
    [Test]
    public void an_abstract_type_cannot_be_registered()
    {
      Executing(() => Resolver.AddDependency<ISimple, SimpleAbstract>()).ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void an_interface_cannot_be_registered_as_a_concrete_implementation()
    {
      Executing(() => Resolver.AddDependency<ISimple, IAnotherSimple>()).ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void cyclic_dependency_generates_an_error()
    {
      Resolver.AddDependency<RecursiveConstructor>();

      Executing(() => Resolver.Resolve<RecursiveConstructor>()).ShouldThrow<DependencyResolutionException>();
    }

    [Test]
    public void parameters_are_resolved()
    {
      Resolver.AddDependency<ISimple, Simple>();
      Resolver.AddDependency<ISimpleChild, SimpleChild>();

      var prop = ((Simple) Resolver.Resolve<ISimple>()).Property;
      prop.ShouldNotBeNull();
      prop.ShouldBeAssignableTo<SimpleChild>();
    }

    [Test]
    public void registered_concrete_type_is_recognized_as_dependency_implementation()
    {
      Resolver.AddDependency<ISimple, Simple>();

      Resolver.HasDependencyImplementation(typeof(ISimple), typeof(Simple)).ShouldBeTrue();
    }

    [Test]
    public void registering_a_concrete_type_results_in_the_type_being_registered()
    {
      Resolver.AddDependency(typeof(Simple), DependencyLifetime.Transient);
      Resolver.HasDependency(typeof(Simple)).ShouldBeTrue();
    }

    [Test]
    public void registering_a_concrete_type_with_an_unknown_dependency_lifetime_value_results_in__an_error()
    {
      Executing(() => Resolver.AddDependency<Simple>((DependencyLifetime) 999))
        .ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void registering_a_service_type_with_an_unknown_dependency_lifetime_value_results_in__an_error()
    {
      Executing(() => Resolver.AddDependency<ISimple, Simple>((DependencyLifetime) 999))
        .ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void requesting_a_type_with_a_public_constructor_returns_a_new_instance_of_that_type()
    {
      Resolver.AddDependency<ISimple, Simple>();
      Resolver.Resolve<ISimple>().ShouldBeAssignableTo<Simple>();
    }

    [Test]
    public void requesting_a_type_with_no_public_constructor_will_return_a_type_with_the_correct_dependency()
    {
      Resolver.AddDependency<ISimple, Simple>();
      Resolver.AddDependency<IAnother, Another>();

      ((Another) Resolver.Resolve<IAnother>()).Dependent.ShouldBeAssignableTo<Simple>();
    }

    [Test]
    public void the_constructor_with_the_most_matching_arguments_is_used()
    {
      Resolver.AddDependency<ISimple, Simple>();
      Resolver.AddDependency<IAnother, Another>();
      Resolver.AddDependency<TypeWithTwoConstructors>();

      var type = Resolver.Resolve<TypeWithTwoConstructors>();
      type._argOne.ShouldNotBeNull();
      type._argTwo.ShouldNotBeNull();
    }

    [Test]
    public void the_null_value_is_never_registered()
    {
      Resolver.HasDependency(null).ShouldBeFalse();
    }

    [Test]
    public void the_resolved_instance_is_the_same_as_the_registered_instance()
    {
      var objectInstance = new object();

      Resolver.AddDependencyInstance(typeof(object), objectInstance);

      Resolver.Resolve<object>().ShouldBe(objectInstance);
    }
  }
}