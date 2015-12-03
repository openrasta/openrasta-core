using System;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Security;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.Web;

namespace OpenRasta.Tests.Unit.Pipeline.Contributors
{
    [TestFixture]
    public class BasicAuthorizer_Specification : openrasta_context
    {
        [Test]
        [TestCase("username", "password")]
        [TestCase("username", "")]
        [TestCase("", "")]
        [TestCase("username", "password:containing:colons")]
        [TestCase("username", "pɹoʍssɐd ǝpoɔᴉun ɐ sᴉ sᴉɥʇ")]
        public void BasicAuthorizer_Succeeds_For_Valid_Credentials(string username, string password)
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            mockAuthenticationProvider.Setup(ap => ap.GetByUsername(username))
                .Returns(new Credentials() { Username = username, Password = password });
            mockAuthenticationProvider.Setup(ap => ap.ValidatePassword(It.IsAny<Credentials>(), It.IsAny<string>())).Returns(true);

            given_dependency(mockAuthenticationProvider.Object);
            given_pipeline_contributor<BasicAuthorizerContributor>();

            Context.Request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));

            var result = when_sending_notification<KnownStages.IHandlerSelection>();
            result.ShouldBe(PipelineContinuation.Continue);
            Context.User.Identity.Name.ShouldBe(username);
        }

        [Test]
        public void BasicAuthorizer_Returns_Authenticate_Header_For_Invalid_Credentials()
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            mockAuthenticationProvider.Setup(ap => ap.GetByUsername(It.IsAny<string>())).Returns(new Credentials());
            mockAuthenticationProvider.Setup(ap => ap.ValidatePassword(It.IsAny<Credentials>(), It.IsAny<string>())).Returns(false);
            given_dependency(mockAuthenticationProvider.Object);
            given_pipeline_contributor<BasicAuthorizerContributor>();
            Context.Request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("username:password")));
            var result = when_sending_notification<KnownStages.IHandlerSelection>();
            result.ShouldBe(PipelineContinuation.RenderNow);
            Context.OperationResult.ShouldBeOfType<OperationResult.Unauthorized>();
            Context.User.ShouldBe(null);
        }

        [Test]
        [TestCase("Basic THIS_IS_NOT_A_BASE64_ENCODED_CREDENTIAL")]
        [TestCase("Invalid Header")]
        public void BasicAuthorizer_Returns_Unauthorized_When_Header_Is_Invalid(string header)
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            given_dependency(mockAuthenticationProvider.Object);
            given_pipeline_contributor<BasicAuthorizerContributor>();

            Context.Request.Headers.Add("Authorization", header);

            var result = when_sending_notification<KnownStages.IHandlerSelection>();
            result.ShouldBe(PipelineContinuation.RenderNow);
            Context.OperationResult.ShouldBeOfType<OperationResult.Unauthorized>();
        }

        [Test]
        public void BasicAuthorizer_Sends_WwwAuthenticate_For_Missing_Credentials()
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            given_dependency(mockAuthenticationProvider.Object);
            given_pipeline_contributor<BasicAuthorizerContributor>();

            when_sending_notification<KnownStages.IHandlerSelection>();
            when_sending_notification<KnownStages.IOperationResultInvocation>();
            var header = String.Format("Basic realm=\"{0}\"", BasicAuthenticationRequiredHeader.DEFAULT_REALM);
            Context.Response.Headers["WWW-Authenticate"].ShouldBe(header);
        }

        [Test]
        public void BasicAuthorizer_Returns_NotAuthorized_For_Missing_Credentials()
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            given_dependency(mockAuthenticationProvider.Object);
            given_pipeline_contributor<BasicAuthorizerContributor>();

            var result = when_sending_notification<KnownStages.IHandlerSelection>();
            result.ShouldBe(PipelineContinuation.RenderNow);
            Context.OperationResult.ShouldBeOfType<OperationResult.Unauthorized>();
        }
    }
}
