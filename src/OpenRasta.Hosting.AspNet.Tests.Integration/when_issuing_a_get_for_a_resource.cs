using System.Net;
using System.Text;
using NUnit.Framework;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
  public class when_issuing_a_get_for_a_resource : aspnet_server_context
  {
    public when_issuing_a_get_for_a_resource()
    {
      ConfigureServer(
        () =>
        {
          ResourceSpace.Has.ResourcesOfType<Customer>()
            .AtUri("/{customerId}")
            .HandledBy<CustomerHandler>();
          
          ResourceSpace.Has.ResourcesOfType<object>().WithoutUri.AsJsonNewtonsoft();
        });
    }

    [Test]
    public void the_request_is_matched_to_the_parameter()
    {
      GivenATextRequest("PATCH", "/3", "new customer name", "UTF-16");
      GivenTheResponseIsInEncoding(Encoding.ASCII);

      TheResponseAsString.ShouldBe("new customer name");
      TheResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
      TheResponse.Headers["Location"].ShouldBe("http://127.0.0.1:6688/3");

      TheResponse.ContentType.ShouldContain("text/plain");
    }
    
  }
}