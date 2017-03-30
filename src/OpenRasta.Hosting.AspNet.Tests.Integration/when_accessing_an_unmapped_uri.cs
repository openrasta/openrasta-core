using System.Net;
using System.Text;
using NUnit.Framework;
using OpenRasta.Configuration;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class when_accessing_an_unmapped_uri : aspnet_server_context
    {
        public when_accessing_an_unmapped_uri()
        {
            ConfigureServer(() => ResourceSpace.Has.ResourcesOfType<Customer>()
                .AtUri("/customer/{customerId}")
                .HandledBy<CustomerHandler>());
        }

        [Test]
        public void openrasta_doesnt_process_the_request()
        {
            GivenARequest("GET", "/mappedCustomers");
            GivenTheResponseIsInEncoding(Encoding.ASCII);

            TheResponse.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        }
    }
}