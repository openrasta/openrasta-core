using System;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Web.UriDecorators;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registering_uri_decorators : configuration_context
  {
    [Test]
    public void a_dependency_is_added_to_the_meta_model()
    {
      ResourceSpaceUses.UriDecorator<TestUriDecorator>();

      var model = MetaModel.CustomRegistrations.OfType<DependencyRegistrationModel>().FirstOrDefault();
      model.ShouldNotBeNull();
      model.ConcreteType.ShouldBe(typeof(TestUriDecorator));
    }

    public class TestUriDecorator : IUriDecorator
    {
      public bool Parse(Uri uri, out Uri processedUri)
      {
        processedUri = null;
        return false;
      }

      public void Apply()
      {
      }
    }
  }
}