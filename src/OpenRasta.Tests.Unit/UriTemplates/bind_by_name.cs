using System;
using System.Collections.Specialized;
using NUnit.Framework;
using OpenRasta.Collections;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class bind_by_name : uritemplate_context
  {
    [Test]
    public void a_wildcard_is_not_generated()
    {
      var baseUri = new Uri("http://localhost");
      var variableValues = new NameValueCollection().With("state", "washington").With("CitY",
        "seattle");

      new UriTemplate("weather/{state}/{city}/*").BindByName(baseUri, variableValues)
        .ShouldBe("http://localhost/weather/washington/seattle/".ToUri());
    }

    [Test]
    public void the_variable_names_are_not_case_sensitive()
    {
      var baseUri = new Uri("http://localhost");
      var variableValues = new NameValueCollection().With("StAte", "washington").With("CitY",
        "seattle");

      new UriTemplate("weather/{state}/{city}/").BindByName(baseUri, variableValues)
        .ShouldBe("http://localhost/weather/washington/seattle/".ToUri());
    }

    [Test]
    public void the_variables_are_replaced_in_the_generated_uri()
    {
      var variableValues = new NameValueCollection().With("state", "washington").With("city",
        "seattle");

      new UriTemplate("weather/{state}/{city}/").BindByName("http://localhost".ToUri(), variableValues)
        .ShouldBe(new Uri("http://localhost/weather/washington/seattle/"));
    }
  }
}