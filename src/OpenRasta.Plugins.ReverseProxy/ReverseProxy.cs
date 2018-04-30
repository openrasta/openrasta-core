using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
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
      _httpClient = new Lazy<HttpClient>(() =>
        {
          var client = options.HttpClient.Factory(options.HttpClient.Handler());
          client.Timeout = Timeout.InfiniteTimeSpan;
          
          return client;
        },
        LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<ReverseProxyResponse> Send(ICommunicationContext context, string target)
    {
      var proxyTargetUri = GetProxyTargetUri(context.PipelineData.SelectedResource.Results.Single(), target);

      var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), proxyTargetUri)
      {
        Version = new Version(2, 0)
      };


      PrepareRequestBody(context, requestMessage);
      PrepareRequestHeaders(context, requestMessage, _options.FrowardedHeaders.ConvertLegacyHeaders);

      var viaIdentifier = PrepareViaHeader(context, requestMessage);

      var cts = new CancellationTokenSource();
      cts.CancelAfter(_timeout);
      var timeoutToken = cts.Token;

      try
      {
        var responseMessage = await _httpClient.Value.SendAsync(
          requestMessage,
          HttpCompletionOption.ResponseHeadersRead,
          timeoutToken
        );
        return new ReverseProxyResponse(requestMessage, responseMessage, viaIdentifier);
      }
      catch (TaskCanceledException e) when (timeoutToken.IsCancellationRequested || e.CancellationToken == timeoutToken)
      {
        // Note we check both cancellation token because mono/fullfx/core don't have the same behaviour
        return new ReverseProxyResponse(requestMessage, via: viaIdentifier, error: e, statusCode: 504);
      }
      catch (HttpRequestException e)
      {
        context.ServerErrors.Add(new Error {Exception = e, Title = $"Reverse Proxy failed to connect."});
        return new ReverseProxyResponse(requestMessage, via: viaIdentifier, error: e, statusCode: 502);
      }
    }

    string PrepareViaHeader(ICommunicationContext context, HttpRequestMessage requestMessage)
    {
      var viaIdentifier = _options.Via.Pseudonym ?? $"{context.Request.Uri.Host}:{context.Request.Uri.Port}";
      requestMessage.Headers.Add("via", $"1.1 {viaIdentifier}");
      return viaIdentifier;
    }

    static void PrepareRequestBody(ICommunicationContext context, HttpRequestMessage requestMessage)
    {
      var hasContentLength = context.Request.Headers.ContentLength > 0;
      var hasTe = context.Request.Headers.ContainsKey("transfer-encoding");
      if (hasContentLength || hasTe)
        requestMessage.Content = new StreamContent(context.Request.Entity.Stream);
    }

    static void PrepareRequestHeaders(ICommunicationContext context, HttpRequestMessage request,
      bool convertLegacyHeaders)
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

        if (HttpHeaderClassification.IsMicrosoftHttpContentHeader(header.Key))
        {
          if (request.Content == null) continue;
            request.Content.Headers.Add(header.Key, header.Value);
        }
        else if (!HttpHeaderClassification.IsHopByHopHeader(header.Key))
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