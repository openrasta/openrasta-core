using System;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.TypeSystem;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registering_resources : configuration_context
  {
    [Test]
    public void a_resource_by_generic_type_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>();

      MetaModel.ResourceRegistrations.Count.ShouldBe(1);
      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBe(typeof(Customer));
    }

    [Test]
    public void a_resource_by_name_is_registered()
    {
      ResourceSpaceHas.ResourcesNamed("customers");

      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBe("customers");
    }

    [Test]
    public void a_resource_by_type_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType(typeof(Customer));

      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBe(typeof(Customer));
    }

    [Test]
    public void a_resource_with_any_key_is_registered()
    {
      var key = new object();
      ResourceSpaceHas.ResourcesWithKey(key);

      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBeSameAs(key);
    }

    [Test]
    public void a_resource_with_IType_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType(TypeSystems.Default.FromClr(typeof(Customer)));

      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBeAssignableTo<IType>().Name.ShouldBe(
          "Customer");
    }

    [Test]
    public void cannot_execute_registration_on_null_IHas()
    {
      Executing(() => ((IHas) null).ResourcesWithKey(new object())).ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void a_resource_of_type_strict_is_registered_as_a_strict_type()
    {
      ResourceSpaceHas.ResourcesOfType<Strictly<Customer>>();

      MetaModel.ResourceRegistrations[0].ResourceKey.ShouldBeAssignableTo<Type>().ShouldBe(typeof(Customer));
      MetaModel.ResourceRegistrations[0].IsStrictRegistration.ShouldBeTrue();
    }

    [Test]
    public void cannot_register_a_resource_with_a_null_key()
    {
      Executing(() => ResourceSpaceHas.ResourcesWithKey(null)).ShouldThrow<ArgumentNullException>();
    }
  }
}