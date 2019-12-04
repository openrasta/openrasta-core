using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class JsonLdCodecWriter : IMediaTypeWriterAsync
  {
    readonly FastUriGenerator _uris;
    readonly ICommunicationContext _context;
    readonly IResponse _responseMessage;
    readonly IMetaModelRepository _models;
    string _apiDocumentationLink;
    static readonly string _apiDocumentationRel = $"{Vocabularies.Hydra.Uri}apiDocumentation";

    public JsonLdCodecWriter(FastUriGenerator uris, ICommunicationContext context, IResponse responseMessage, IMetaModelRepository models)
    {
      _uris = uris;
      _context = context;
      _responseMessage = responseMessage;
      _models = models;
      _apiDocumentationLink = uris.CreateUri<ApiDocumentation>(_context.ApplicationBaseUri);
    }

    public object Configuration { get; set; }

    Uri BaseUri => _context.ApplicationBaseUri;

    public Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      _responseMessage.Headers.Add("link", $"<{_apiDocumentationLink}>; rel=\"{_apiDocumentationRel}\"");

      var resourceSelectedByUri = _context.PipelineData.SelectedResource;

      var serializerFunc = resourceSelectedByUri.ResourceModel.Hydra().SerializeFunc;
      
      if (serializerFunc == null)
      {
        serializerFunc = GetFuncFromResponseResourceType(_context.Response.Entity.Instance);
        if (serializerFunc == null)
          throw new InvalidOperationException($"Hydra serialiser not found for object of type {entity?.GetType()}");
      }

      var typeToTypeGen = _models.ResourceRegistrations
        .Where(res => res.ResourceType != null && res.Hydra().TypeFunc != null)
        .ToLookup(res => res.ResourceType, res => res.Hydra().TypeFunc);
      
      string renderTypeNode(object resource)
      {
        var convertedString = typeToTypeGen[resource.GetType()].First()(resource);
        if (convertedString.StartsWith("http://") || convertedString.StartsWith("https://") || IsCurie(convertedString))
          return convertedString;
        return BaseUri + convertedString;
      }
      
      return serializerFunc(entity, new SerializationContext
      {
        BaseUri = BaseUri,
        UriGenerator = resource => _uris.CreateUri(resource, _context.ApplicationBaseUri),
        TypeGenerator =  renderTypeNode
      }, response.Stream);
    }

    static bool IsCurie(string convertedString) => Regex.IsMatch(convertedString, "^\\w+:");

    Func<object, SerializationContext, Stream, Task> GetFuncFromResponseResourceType(object entityInstance)
    {
      if (entityInstance == null) return null;

      return _models.TryGetResourceModel(entityInstance.GetType(), out var responseEntityResourceModel) 
        ? responseEntityResourceModel.Hydra().SerializeFunc
        : null;
    }
  }
}