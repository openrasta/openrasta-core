using System.Linq;
using OpenRasta.Plugins.Caching.Pipeline;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.conditionals
{
    public class invalid_combination : contexts.caching
    {
        public invalid_combination()
        {
            given_resource<TestResource>(map => map.Etag(_ => "v1"));
            given_request_header("if-match", Etag.StrongEtag("v1"));
            given_request_header("if-none-match", Etag.StrongEtag("v1"));
            when_executing_request("/TestResource");
        }

        [Fact]
        public void warning_header_generated()
        {
            response.Headers["warning"]
                .Split(',')
                .Any(_ => _.Trim().StartsWith("199 If-Lolcat")).ShouldBeTrue();
        }
    }
}