using System;
using System.Collections.Specialized;
using NUnit.Framework;
using OpenRasta.Collections;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class when_using_fragment_identifiers : uritemplate_context
  {
    [Test]
    public void BindingReturnsGeneratedUri()
    {
      var baseUri = new Uri("http://localhost");
      var variableValues = new NameValueCollection().With("state", "washington").With("City",
        "seattle");

      var bindByName = new UriTemplate("weather#{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather#washington/seattle/");

      bindByName = new UriTemplate("weather/#{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather/#washington/seattle/");

      bindByName = new UriTemplate("weather/#/{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather/#/washington/seattle/");
    }
  }
}