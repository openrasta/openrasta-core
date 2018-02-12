using System;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.TypeSystem;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registring_handlers_for_resources_with_URIs : configuration_context
  {
    [Test]
    public void a_handler_from_generic_type_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType<Frodo>()
          .AtUri("/theshrine")
          .HandledBy<CustomerHandler>();

      FirstRegistration.Handlers[0].Type.Name.ShouldBe( "CustomerHandler");
    }

    [Test]
    public void a_handler_from_itype_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType(typeof(Frodo))
          .AtUri("/theshrine")
          .HandledBy(TypeSystems.Default.FromClr(typeof(CustomerHandler)));
      FirstRegistration.Handlers[0].Type.Name.ShouldBe( "CustomerHandler");
    }

    [Test]
    public void a_handler_from_type_instance_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType(typeof(Frodo))
          .AtUri("/theshrine")
          .HandledBy(typeof(CustomerHandler));
      FirstRegistration.Handlers[0].Type.Name.ShouldBe( "CustomerHandler");
    }

    [Test]
    public void cannot_add_a_null_handler()
    {
      Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy((IType) null))
          .ShouldThrow<ArgumentNullException>();
      Executing(() => ResourceSpaceHas.ResourcesOfType<Frodo>().AtUri("/theshrine").HandledBy((Type) null))
          .ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void two_handlers_can_be_added()
    {
      ResourceSpaceHas.ResourcesOfType(typeof(Frodo))
          .AtUri("/theshrine")
          .HandledBy<CustomerHandler>()
          .And
          .HandledBy<object>();

      FirstRegistration.Handlers[0].Type.Name.ShouldBe( "CustomerHandler");
      FirstRegistration.Handlers[1].Type.Name.ShouldBe( "Object");
    }
  }
}