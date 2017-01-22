using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Testing;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    [TestFixture]
    public class when_matching_verbs
    {
        [Test]
        public void a_wildcard_matches()
        {
            new HttpHandlerRegistration("*", "/", "")
                .Matches("GET", new Uri("http://localhost/", UriKind.Absolute))
                .ShouldBeTrue();
        }
        [Test]
        public void an_http_verb_is_matched()
        {
            new HttpHandlerRegistration("GET,POST", "/", "")
                .Matches("POST", new Uri("http://localhost"))
                .ShouldBeTrue();
        }
        [Test]
        public void another_http_verb_is_not_matched()
        {
            new HttpHandlerRegistration("GET,POST", "/", "")
                .Matches("PUT", new Uri("http://localhost"))
                .ShouldBeFalse();
        }

    }
}
