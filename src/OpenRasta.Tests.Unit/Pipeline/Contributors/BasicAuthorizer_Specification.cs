using System;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Security;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.Web;
using Shouldly;

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
          //return valueToAnalyse;
          ShouldBeTestExtensions.ShouldBe(Context.User.Identity.Name, username);
          //return valueToAnalyse;
        }
    }
}
