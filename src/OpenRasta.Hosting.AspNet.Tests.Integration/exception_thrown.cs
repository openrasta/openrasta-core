using System.Net;
using System.Text;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.IO;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class exception_thrown : aspnet_server_context
    {
        public exception_thrown()
        {
            ConfigureServer(()=>ResourceSpace.Has
                .ResourcesOfType<Customer>()
                .AtUri("/customer")
                .And.AtUri("/customer/success").Named("success")
                .HandledBy<ThrowHandler>()
                );
        }
        [Test]
        public void second_request_is_successful()
        {
            GivenARequest("GET", "/customer");
            TheResponse.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            GivenARequest("GET", "/customer/success");
            TheResponseAsString.ShouldBe("hello");
            TheResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        }
    }
}