using OpenRasta.Plugins.Caching.Pipeline;
using OpenRasta.Web;
using Shouldly;

namespace Tests.Plugins.Caching
{
  public static class TestExtensions
  {
    public static IResponse ShouldHaveVariantEtag(this IResponse response, string etag)
    {
      response.Headers.ContainsKey(CachingHttpHeaders.Etag).ShouldBeTrue();
      response.Headers[CachingHttpHeaders.Etag].EndsWith(":" + etag + '"').ShouldBeTrue();
      return response;
    }
  }
}