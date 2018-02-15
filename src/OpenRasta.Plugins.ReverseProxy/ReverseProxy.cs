using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Web;
using HttpMethod = System.Net.Http.HttpMethod;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ReverseProxy
  {
    readonly IMetaModelRepository _modelRepository;
    Lazy<HttpClient> _httpClient;

    public ReverseProxy(ReverseProxyOptions options, IMetaModelRepository modelRepository)
    {
      _modelRepository = modelRepository;
      _httpClient = new Lazy<HttpClient>(
          () => new HttpClient(options.HttpMessageHandler()),
          LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<HttpResponseMessage> Send(ICommunicationContext context, string target)
    {
      var sourceBaseUri = new Uri(context.Request.Uri, "/");
      var targetTemplate = new UriTemplate(target);
      var requestQs = context.PipelineData.SelectedResource.Results.Single();

      //var requestQueryStrings = context.Request.Uri.
      var targetUri = targetTemplate.BindByName(sourceBaseUri,
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
          string.Join("&", requestQs.Match.QueryString
          .Where(qs=>!sourceTemplateQsKeysWithVars.Contains(qs.Key, StringComparer.OrdinalIgnoreCase))
          .Select(qs=>qs.ToString()));

      if (requestQueryNotMappedToSourceTemplateVars.Length > 0)
      {
        targetUriBuilder.Query += !targetUriBuilder.Query.StartsWith("?") ? "?" : "&";
        
        targetUriBuilder.Query += requestQueryNotMappedToSourceTemplateVars;
      }

      var request = new HttpRequestMessage
      {
          RequestUri = targetUriBuilder.Uri,
          Method = new HttpMethod(context.Request.HttpMethod),
          Content = { }
      };

      foreach (var header in context.Request.Headers)
        request.Headers.Add(header.Key, header.Value);

      return await _httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    }
  }
}