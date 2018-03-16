using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Web;
using HttpMethod = System.Net.Http.HttpMethod;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxy
  {
    readonly ReverseProxyOptions _options;
    readonly Lazy<HttpClient> _httpClient;
    readonly TimeSpan _timeout;

    public ReverseProxy(ReverseProxyOptions options)
    {
      _options = options;
      _timeout = options.Timeout;
      _httpClient = new Lazy<HttpClient>(() => new HttpClient(_options.HttpMessageHandler())
        {
          Timeout = Timeout.InfiniteTimeSpan
        },
        LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<HttpResponseMessage> Send(ICommunicationContext context, string target)
    {
      var proxyTargetUri = GetProxyTargetUri(context.PipelineData.SelectedResource.Results.Single(), target);
      var request = new HttpRequestMessage
      {
        Method = new HttpMethod(context.Request.HttpMethod),
        Content = new StreamContent(context.Request.Entity.Stream)
      };

      CopyHeaders(context, request, _options.FrowardedHeaders.ConvertLegacyHeaders);

      var headers = request.Headers;
      var identifier = _options.Via.Pseudonym ?? $"{context.Request.Uri.Host}:{context.Request.Uri.Port}";
      headers.Add("via", $"1.1 {identifier}");

      request.RequestUri = proxyTargetUri;
      var cts = new CancellationTokenSource();
      cts.CancelAfter(_timeout);
      var token = cts.Token;
      try
      {
        var httpResponseMessage = await _httpClient.Value.SendAsync(
          request,
          HttpCompletionOption.ResponseHeadersRead,
          token
        );
        httpResponseMessage.Headers.Via.Add(new ViaHeaderValue("1.1", identifier));
        return httpResponseMessage;
      }
      catch (TaskCanceledException e) when (token.IsCancellationRequested || e.CancellationToken == token)
      {
        return new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.GatewayTimeout
        };
      }
      catch (Exception e)
      {
        throw new ReverseProxyFailedRequestException(request, e);
      }
      finally
      {
        request.Dispose();
      }
    }

    static void CopyHeaders(ICommunicationContext context, HttpRequestMessage request, bool convertLegacyHeaders)
    {
      StringBuilder legacyForward = null;

      void appendParameter(string key, string value)
      {
        if (legacyForward == null) legacyForward = new StringBuilder();
        if (legacyForward.Length > 0) legacyForward.Append(";");
        legacyForward.Append(key).Append("=").Append(value);
      }

      foreach (var header in context.Request.Headers)
      {
        if (convertLegacyHeaders)
        {
          if (header.Key.Equals("X-Forwarded-Host", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("host", header.Value);
            continue;
          }

          if (header.Key.Equals("X-Forwarded-For", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("for", header.Value);
            continue;
          }

          if (header.Key.Equals("X-Forwarded-Proto", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("proto", header.Value);
            continue;
          }
        }

        if (header.Key.Equals("host", StringComparison.OrdinalIgnoreCase)) continue;
        if (header.Key.StartsWith("Content-"))
          request.Content.Headers.Add(header.Key, header.Value);
        else
          request.Headers.Add(header.Key, header.Value);
      }

      if (convertLegacyHeaders && legacyForward?.Length > 0)
      {
        request.Headers.Add("forwarded", legacyForward.ToString());
      }

      request.Headers.Add("forwarded", CurrentForwarded(context));
    }

    static Uri GetProxyTargetUri(TemplatedUriMatch requestUriMatch, string target)
    {
      var destinationBaseUri = new Uri(new Uri(target), "/");
      var targetTemplate = new UriTemplate(target);
      var requestQs = requestUriMatch;

      var targetUri = targetTemplate.BindByName(
        destinationBaseUri,
        new NameValueCollection
        {
          requestQs.Match.PathSegmentVariables,
          requestQs.Match.QueryStringVariables
        });

      var targetUriBuilder = new UriBuilder(targetUri);

      var sourceTemplateQsKeysWithVars = requestQs.Match.Template.QueryString
        .Where(qs => qs.Type == UriTemplate.SegmentType.Variable)
        .Select(qs => qs.Key)
        .ToList();

      var requestQueryNotMappedToSourceTemplateVars =
        string.Join("&",
          requestQs.Match.QueryString
            .Where(qs => !sourceTemplateQsKeysWithVars.Contains(qs.Key, StringComparer.OrdinalIgnoreCase))
            .Select(qs => qs.ToString()));

      if (requestQueryNotMappedToSourceTemplateVars.Length > 0)
      {
        var query = targetUriBuilder.Query;
        if (query.StartsWith("?")) query = query.Substring(1);
        if (query.Length > 0) query += "&";
        query += requestQueryNotMappedToSourceTemplateVars;
        targetUriBuilder.Query = query;
      }

      var proxyTargetUri = targetUriBuilder.Uri;
      return proxyTargetUri;
    }

    static string CurrentForwarded(ICommunicationContext context)
    {
      return $"proto={context.Request.Uri.Scheme};host={context.Request.Uri.Host}";
    }
  }
}