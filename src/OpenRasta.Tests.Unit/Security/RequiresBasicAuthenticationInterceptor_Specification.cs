using System;
using System.Diagnostics;
using System.Security.Principal;
using Moq;
using NUnit.Framework;
using OpenRasta.Hosting.InMemory;
using OpenRasta.OperationModel;
using OpenRasta.Security;
using OpenRasta.Testing;
using OpenRasta.Web;

namespace RequiresBasicAuthenticationInterceptor_Specification
{
    public class when_the_user_is_not_authenticated : context
    {
        [Test]
        public void execution_is_denied()
        {
            var context = new InMemoryCommunicationContext();
            const string REALM = "Test Realm";
            var authenticationInterceptor = new RequiresBasicAuthenticationInterceptor(context, REALM);
            authenticationInterceptor.BeforeExecute(new Mock<IOperation>().Object).ShouldBeFalse();
            context.OperationResult.ShouldBeOfType<OperationResult.Unauthorized>();
            var expectedHeader = String.Format("Basic realm=\"{0}\"", REALM);
            context.Response.Headers["WWW-Authenticate"].ShouldBe(expectedHeader);
        }
    }
}