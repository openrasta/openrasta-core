using OpenRasta.Plugins.Caching.Pipeline;
using OpenRasta.Web;
using Shouldly;

namespace Tests.Plugins.Caching
{
    public static class TestExtensions
    {
        public static IResponse ShouldHaveVariantEtag(this IResponse response, string etag)
        {
            response.Headers.ContainsKey(CachingHttpHeaders.ETAG).ShouldBeTrue();
            response.Headers[CachingHttpHeaders.ETAG].EndsWith(":" + etag + '"').ShouldBeTrue();
            return response;
        }
    }
}