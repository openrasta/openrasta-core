using OpenRasta.Plugins.Caching;
using OpenRasta.Plugins.Caching.Configuration;
using Shouldly;
using Tests.Plugins.Caching.last_modified;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
    public class unmodified : contexts.caching
    {
        public unmodified()
        {
            given_current_time(now);
             
            given_resource(resource=>resource.Map<ResourceWithLastModified>().LastModified(_=> _.LastModified),
                "/resource", new ResourceWithLastModified { LastModified = now - 1.Minutes() });

            given_request_header("if-modified-since", now);
            when_executing_request("/resource");
        }

        [Fact]
        public void request_successful()
        {
            response.StatusCode.ShouldBe(304);
        }

        [Fact]
        public void last_modified_set()
        {
            should_be_date(response.Headers["last-modified"], now - 1.Minutes());
        }
    }
}