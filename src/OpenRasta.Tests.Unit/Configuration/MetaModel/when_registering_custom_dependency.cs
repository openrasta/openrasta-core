using System.Linq;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using OpenRasta.Web;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registering_custom_dependency : configuration_context
  {
    [Test]
    public void a_dependency_registration_is_added_to_the_metamodel()
    {
      ResourceSpaceUses.CustomDependency<IUriResolver, TemplatedUriResolver>(DependencyLifetime.Singleton);

      var first = MetaModel.CustomRegistrations.OfType<DependencyRegistrationModel>()
          .ShouldHaveSingleItem();
      first.ConcreteType.ShouldBe(typeof(TemplatedUriResolver));
      first.ServiceType.ShouldBe(typeof(IUriResolver));
      first.Lifetime.ShouldBe(DependencyLifetime.Singleton);
    }
  }
}