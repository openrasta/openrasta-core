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
    readonly Func<string, HttpClient> _httpClient;
    readonly Action<ICommunicationContext, HttpRequestMessage> _onSend;
    readonly Action<ReverseProxyResponse> _onProxyResponse;
    readonly TimeSpan _timeout;
    readonly bool _convertForwardedHeaders;
    readonly string _viaIdentifier;

    public ReverseProxy(TimeSpan requestTimeout, bool convertForwardedHeaders, string viaIdentifier,
      Func<string, HttpClient> clientFactory,
      Action<ICommunicationContext, HttpRequestMessage> onSend,
      Action<ReverseProxyResponse> onProxyResponse)
    {
      _timeout = requestTimeout;
      _httpClient = clientFactory;
      _onSend = onSend;
      _onProxyResponse = onProxyResponse;
      _convertForwardedHeaders = convertForwardedHeaders;
      _viaIdentifier = viaIdentifier;
    }

    public async Task<ReverseProxyResponse> Send(ICommunicationContext context, string target)
    {
      var proxyTargetUri = GetProxyTargetUri(context.PipelineData.SelectedResource.Results.Single(), target);

      var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), proxyTargetUri)
      {
        Version = new Version(2, 0)
      };

      PrepareRequestBody(context, requestMessage);
      PrepareRequestHeaders(context, requestMessage, _convertForwardedHeaders);

      var viaIdentifier = AppendViaHeaderToRequest(context, requestMessage);


      var cts = new CancellationTokenSource();
      cts.CancelAfter(_timeout);
      var timeoutToken = cts.Token;


      ReverseProxyResponse reverseProxyResponse;
      
      try
      {
        _onSend?.Invoke(context, requestMessage);
        var httpClient = _httpClient(requestMessage.RequestUri.Host);
        var responseMessage = await httpClient.SendAsync(
          requestMessage,
          HttpCompletionOption.ResponseHeadersRead,
          timeoutToken
        );
        reverseProxyResponse = new ReverseProxyResponse(requestMessage, responseMessage, viaIdentifier);
      }
      catch (TaskCanceledException e) when (timeoutToken.IsCancellationRequested || e.CancellationToken == timeoutToken)
      {
        // Note we check both cancellation token because mono/fullfx/core don't have the same behaviour
        reverseProxyResponse = new ReverseProxyResponse(requestMessage, via: null, error: e, statusCode: 504);
      }
      catch (HttpRequestException e)
      {
        context.ServerErrors.Add(new Error {Exception = e, Title = $"Reverse Proxy failed to connect."});
        reverseProxyResponse = new ReverseProxyResponse(requestMessage, via: null, error: e, statusCode: 502);
      }

      _onProxyResponse?.Invoke(reverseProxyResponse);
      return reverseProxyResponse;
    }

    string AppendViaHeaderToRequest(ICommunicationContext context, HttpRequestMessage requestMessage)
    {
      var viaIdentifier = _viaIdentifier ?? $"{context.Request.Uri.Host}:{context.Request.Uri.Port}";
      requestMessage.Headers.Add("via", $"{context.PipelineData.Owin.RequestProtocol} {viaIdentifier}");
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

      var orRequest = context.Request;
      foreach (var headerKey in orRequest.Headers.Keys)
      {
        if (convertLegacyHeaders)
        {
          if (headerKey.Equals("X-Forwarded-Host", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("host", orRequest.Headers[headerKey]);
            continue;
          }

          if (headerKey.Equals("X-Forwarded-For", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("for", orRequest.Headers[headerKey]);
            continue;
          }

          if (headerKey.Equals("X-Forwarded-Proto", StringComparison.OrdinalIgnoreCase))
          {
            appendParameter("proto", orRequest.Headers[headerKey]);
            continue;
          }

          if (headerKey.Equals("X-Forwarded-Base", StringComparison.OrdinalIgnoreCase))
          {
            var baseHeaderValue = orRequest.Headers[headerKey];
            var baseVal = $"\"{(baseHeaderValue[0] != '/' ? "/" + baseHeaderValue : baseHeaderValue)}\"";
            appendParameter("base", baseVal);
            continue;
          }
        }

        if (headerKey.Equals("host", StringComparison.OrdinalIgnoreCase)) continue;

        if (HttpHeaderClassification.IsMicrosoftHttpContentHeader(headerKey))
        {
          if (request.Content == null) continue;

          request.Content.Headers.Add(headerKey, orRequest.Headers.GetValues(headerKey));
        }
        else if (!HttpHeaderClassification.IsHopByHopHeader(headerKey))
          request.Headers.Add(headerKey, orRequest.Headers.GetValues(headerKey));
      }

      if (convertLegacyHeaders && legacyForward?.Length > 0)
      {
        request.Headers.Add("forwarded", legacyForward.ToString());
      }

      request.Headers.Add("forwarded", CurrentForwarded(context));
    }

    static Uri GetProxyTargetUri(TemplatedUriMatch requestUriMatch, string target,
      Action<UriBuilder> overrideUri = null)
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

      overrideUri?.Invoke(targetUriBuilder);
      var proxyTargetUri = targetUriBuilder.Uri;
      return proxyTargetUri;
    }

    static string CurrentForwarded(ICommunicationContext context)
    {
      return $"proto={context.Request.Uri.Scheme};host={context.Request.Uri.Host}";
    }
  }
}