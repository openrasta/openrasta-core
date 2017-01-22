using System.Net;
using System.Text;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Testing;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class when_accessing_a_uri_meant_for_a_handler : aspnet_server_context
    {
        public when_accessing_a_uri_meant_for_a_handler()
        {
            ConfigureServer(() => ResourceSpace.Has.ResourcesOfType<Customer>()
                .AtUri("/customer/{customerId}")
                .HandledBy<CustomerHandler>());
        }

        [Test]
        public void oepnrasta_doesnt_process_the_request()
        {
            GivenARequest("GET", "/customer/3.notimplemented");
            GivenTheResponseIsInEncoding(Encoding.ASCII);

            TheResponse.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
        }
    }
}