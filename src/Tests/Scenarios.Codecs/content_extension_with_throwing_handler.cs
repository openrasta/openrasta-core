using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using OpenRasta.Web.UriDecorators;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerSelection.Scenarios.Codecs
{
  public class content_extension_with_throwing_handler
  {
    [GitHubIssue(15)]
    public async Task content_type_in_extension_overrides_conneg()
    {
      var server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.UriDecorator<ContentTypeExtensionUriDecorator>();
        
        ResourceSpace.Has.ResourcesOfType<object>()
          .WithoutUri
          .AsXmlDataContract().ForMediaType(MediaType.Xml.WithQuality(0.9f)).ForExtension("xml")
          .And
          .AsJsonDataContract().ForMediaType(MediaType.Json.WithQuality(1.0f)).ForExtension("json");

        ResourceSpace.Has.ResourcesOfType<MyResource>()
          .AtUri("/stuff")
          .HandledBy<MyResourceHandler>();
      });
      var response = await server.Get("/stuff.xml");
      response.StatusCode.ShouldBe(500);
      response.Entity.ContentType.MediaType.ShouldBe(MediaType.Xml.MediaType);
    }

    class MyResourceHandler
    {
      public string Get()
      {
        throw new InvalidOperationException();
      }
    }
    class MyResource{}
  }
}