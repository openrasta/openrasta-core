using System.Net;
using System.Text;
using NUnit.Framework;
using OpenRasta.Configuration;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
  public class when_issueing_a_get_for_a_resource : aspnet_server_context
  {
    public when_issueing_a_get_for_a_resource()
    {
      ConfigureServer(
        () => ResourceSpace.Has.ResourcesOfType<Customer>()
          .AtUri("/{customerId}")
          .HandledBy<CustomerHandler>());
    }

    [Test]
    public void the_request_is_matched_to_the_parameter()
    {
      GivenATextRequest("PATCH", "/3", "new customer name", "UTF-16");
      GivenTheResponseIsInEncoding(Encoding.ASCII);

      TheResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
      TheResponseAsString.ShouldBe("new customer name");
      TheResponse.Headers["Location"].ShouldBe("http://127.0.0.1:6688/3");

      TheResponse.ContentType.ShouldContain("text/plain");
    }
    
  }
}