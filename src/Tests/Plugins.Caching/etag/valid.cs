using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.etag
{
    public class valid : contexts.caching
    {
        public valid()
        {
            given_resource<TestResource>(map => map.Etag(_ => "v1"));

            when_executing_request("/TestResource");
        }

        [Fact]
        public void request_successful()
        {
            response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public void etag_present()
        {
            response.ShouldHaveVariantEtag("v1");
        }
    }
}