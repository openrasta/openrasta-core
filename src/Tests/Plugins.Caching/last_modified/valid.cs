using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.last_modified
{
    public class valid : contexts.caching
    {
        public valid()
        {
            given_resource(map=>map.LastModified(_=>_.LastModified),
                "/resource", new ResourceWithLastModified { LastModified = now });

            when_executing_request("/resource");
        }
        
        [Fact]
        public void request_successful()
        {   
            response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public void last_modified_header_present()
        {
            should_be_date(response.Headers["last-modified"], now);
        }
    }
}