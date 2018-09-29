using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class JsonLdCodecWriter : IMediaTypeWriterAsync
  {
    readonly IUriResolver _uris;
    readonly ICommunicationContext _context;
    readonly IResponse _responseMessage;
    Uri _apiDocumentationLink;
    static readonly string _apiDocumentationRel = $"{Vocabularies.Hydra.Uri}apiDocumentation";

    public JsonLdCodecWriter(IUriResolver uris, ICommunicationContext context, IResponse responseMessage)
    {
      _uris = uris;
      _context = context;
      _responseMessage = responseMessage;
      _apiDocumentationLink = uris.CreateUriFor<ApiDocumentation>();
    }

    public object Configuration { get; set; }

    Uri BaseUri => _context.ApplicationBaseUri;

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      _responseMessage.Headers.Add("link", $"<{_apiDocumentationLink}>; rel=\"{_apiDocumentationRel}\"");

      var func = _context.PipelineData.SelectedResource.ResourceModel.Hydra().SerializeFunc;

      if (func == null) throw new InvalidOperationException($"Hydra serialiser not found for object of type {entity?.GetType()}");
      await func(entity, new SerializationContext
      {
        BaseUri = BaseUri,
        UriGenerator = resource=>_uris.CreateFrom(resource, BaseUri).ToString()
      }, response.Stream);
    }
  }
}