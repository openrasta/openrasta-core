using System;
using System.Net.Http;
using System.Threading;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Configuration.Fluent.Extensions;

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

      Func<HttpClient> clientFactory = () =>
      {
        var client = options.HttpClient.Factory(options.HttpClient.Handler());
        client.Timeout = Timeout.InfiniteTimeSpan;

        return client;
      };

      uses.Dependency(d => d.Singleton(() => new ReverseProxy(
        options.Timeout,
        options.FrowardedHeaders.ConvertLegacyHeaders,
        options.Via.Pseudonym, 
        clientFactory)));
      
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