using System;
using System.Net.Http;
using System.Threading;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;

namespace OpenRasta.Plugins.ReverseProxy
{
  public static class FluentApiExtensions
  {
    public static void ReverseProxyFor(this IUriDefinition uriConfiguration, string uri)
    {
      uriConfiguration
        .HandledBy<ReverseProxyHandler>();

      var target = (IResourceTarget) uriConfiguration;
      target.Resource.ReverseProxyTarget(uri);
    }

    public static T ReverseProxy<T>(this T uses, ReverseProxyOptions options = null) where T : IUses
    {
      options = options ?? new ReverseProxyOptions();

      uses.Dependency(d => d.Singleton(() => new ReverseProxy(
        options.Timeout,
        options.FrowardedHeaders.ConvertLegacyHeaders,
        options.Via.Pseudonym, 
        options.HttpClient.Factory)));
      
      var has = (IHas) uses;
      has.ResourcesOfType<ReverseProxyResponse>()
        .WithoutUri
        .TranscodedBy<ReverseProxyResponseCodec>()
        .ForMediaType("*/*");

      if (options.FrowardedHeaders.RunAsForwardedHost)
        uses.PipelineContributor<RewriteAppBaseUsingForwardedHeaders>();

      return uses;
    }
  }
}