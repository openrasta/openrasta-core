using System;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    [TestFixture]
    public class when_matching_paths
    {
        [Test]
        public void a_wildcard_path_is_matched()
        {
            new HttpHandlerRegistration("*", "*", "")
                .Matches("GET", new Uri("http://localhost/", UriKind.Absolute))
                .ShouldBeTrue();
        }
        [Test]
        public void a_wildcard_path_is_matched_in_a_segment()
        {
            new HttpHandlerRegistration("*", "*.svc", "")
                .Matches("GET", new Uri("http://localhost/service.svc/Value", UriKind.Absolute))
                .ShouldBeTrue();
        }

        [Test]
        public void a_general_wildcard_path_is_not_matched_with_dot_at_end_of_query()
        {
            new HttpHandlerRegistration("*", "*.", "")
                .Matches("GET", new Uri("http://localhost/path?q=blah.", UriKind.Absolute))
                .ShouldBeFalse();
        }
    }
}