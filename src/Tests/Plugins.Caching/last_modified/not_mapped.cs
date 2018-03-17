using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.last_modified
{
    public class not_mapped : contexts.caching
    {
        public not_mapped()
        {
            given_time(now);
            given_resource(
                "/resource",
                new ResourceWithLastModified { LastModified = now });

            when_executing_request("/resource");
        }

        [Fact]
        public void request_successful()
        {
            response.StatusCode.ShouldBe(200);

        }
        [Fact]
        public void header_not_present()
        {
            response.Headers.ContainsKey("last-modified").ShouldBeFalse();
        }
    }
}