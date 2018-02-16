using System;
using System.Net.Http;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.ReverseProxy
{
  public static class FluentApiExtensions
  {
    public static void ReverseProxyFor(this IUriDefinition uriConfiguration, string uri)
    {
      uriConfiguration
          .HandledBy<ReverseProxyHandler>();

      var target = (IResourceTarget)uriConfiguration;
      target.Resource.ReverseProxyTarget(uri);
    }

    public static T ReverseProxy<T>(this T uses, ReverseProxyOptions options = null) where T : IUses
    {
      uses.Dependency(d => d.Singleton((IMetaModelRepository repository) => new ReverseProxy(options, repository)));
      ((IHas)uses)
          .ResourcesOfType<HttpResponseMessage>()
          .WithoutUri
          .TranscodedBy<ReverseProxyCodec>()
          .ForMediaType("*/*");

      if (options.FrowardedHeaders.RunAsForwardedHost)
        uses.PipelineContributor<RewriteAppBaseUsingForwardedHeaders>();
      return uses;
    }
  }
}