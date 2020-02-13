using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Tests.Unit.Fakes;
using Shouldly;

namespace Configuration_Specification
{
  public class when_registering_uris : configuration_context
  {
    IList<UriModel> TheUris
    {
      get { return MetaModel.ResourceRegistrations[0].Uris; }
    }

    [Test]
    public void using_resource_func()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(c => $"/customer/{c.Address}");

      TheUris[0].Uri.ShouldBe("/customer/{Address}");
    }

    [Test]
    public void using_nested_func()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(c => $"/customer/{c.Address.City}");

      TheUris[0].Uri.ShouldBe("/customer/{AddressCity}");
    }
    
    [Test]
    public void using_resource_func_with_var()
    {
      // TODO:Implement this correctly
      var stringVar = "test";
      const string stringConst = "test2";
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(c => $"/customer/{c.Address}/{stringVar}/{stringConst}/{UriFuncProp}");

      TheUris[0].Uri.ShouldBe("/customer/{Address}/test/test2/test3");
    }
    [Test]
    public void using_resource_func_no_args()
    {
      // ReSharper disable once RedundantStringInterpolation
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(c => $"/customer/");

      TheUris[0].Uri.ShouldBe("/customer/");
    }

    [Test]
    public void using_resource_func_no_format()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri(c => "/customer/");

      TheUris[0].Uri.ShouldBe("/customer/");
    }

    public string UriFuncProp => "test3";

    [Test]
    public void a_null_language_defaults_to_the_inviariant_culture()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").InLanguage(null);
      TheUris[0].Language.ShouldBe(CultureInfo.InvariantCulture);
    }

    [Test]
    public void a_resource_can_be_registered_with_no_uri()
    {
      ICodecParentDefinition reg = ResourceSpaceHas.ResourcesOfType<Customer>().WithoutUri;
      TheUris.Count.ShouldBe(0);
    }

    [Test]
    public void a_uri_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer");

      TheUris.Count.ShouldBe(1);
      TheUris[0].Uri.ShouldBe("/customer");
    }

    [Test]
    public void a_uri_language_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").InLanguage("fr");
      TheUris[0].Language.Name.ShouldBe("fr");
    }

    [Test]
    public void a_uri_name_is_registered()
    {
      ResourceSpaceHas.ResourcesOfType<Customer>().AtUri("/customer").Named("default");

      TheUris[0].Name.ShouldBe("default");
    }

    [Test]
    public void can_register_multiple_uris_for_a_resource()
    {
      ResourceSpaceHas.ResourcesOfType<Frodo>()
          .AtUri("/theshire")
          .And
          .AtUri("/lothlorien");

      TheUris.Count.ShouldBe(2);
      TheUris[0].Uri.ShouldBe("/theshire");
      TheUris[1].Uri.ShouldBe("/lothlorien");
    }


    [Test]
    public void cannot_register_a_null_uri_for_a_resource()
    {
      Executing(() => ResourceSpaceHas.ResourcesOfType<Customer>().AtUri((string)null)).ShouldThrow<ArgumentNullException>();
    }
  }
}