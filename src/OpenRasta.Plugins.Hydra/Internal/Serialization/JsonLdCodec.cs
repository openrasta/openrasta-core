using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenRasta.Codecs;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class JsonLdCodec : IMediaTypeWriterAsync
  {
    readonly ICommunicationContext _context;
    readonly IMetaModelRepository _models;
    readonly IResponse _responseMessage;
    readonly IUriResolver _uris;
    Uri _apiDocumentationLink;
    static readonly string _apiDocumentationRel = $"{Vocabularies.Hydra.Uri}apiDocumentation";


    public JsonLdCodec(IUriResolver uris, ICommunicationContext context, IMetaModelRepository models,
      IResponse responseMessage)
    {
      _uris = uris;
      _context = context;
      _models = models;
      _responseMessage = responseMessage;
      _apiDocumentationLink = uris.CreateUriFor<ApiDocumentation>();
    }

    Uri BaseUri => _context.ApplicationBaseUri;

    public object Configuration { get; set; }

    public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
    {
      _responseMessage.Headers.Add("link", $"<{_apiDocumentationLink}>; rel=\"{_apiDocumentationRel}\"");

      if (entity is IEnumerable enumerableEntity)
        entity = ConvertToHydraCollection(enumerableEntity, _context.Request.Uri);
      
      var customConverter = new JsonSerializer
      {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        ContractResolver = new JsonLdContractResolver(BaseUri, _uris, _models),
        TypeNameHandling = TypeNameHandling.None,
        Converters =
        {
          new StringEnumConverter(),
          new JsonLdTypeRefConverter(_models),
          new HydraUriModelConverter(BaseUri),
          new RdfPropertyFromJsonPropertyConverter(_models),
          new ContextDocumentConverter()
        }
      };

      using (var writer = new StreamWriter(response.Stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        bufferSize: 1024, leaveOpen: true))
      using (var jsonWriter = new JsonTextWriter(writer))
      {
        customConverter.Serialize(jsonWriter, entity);
      }
    }

    Collection ConvertToHydraCollection(IEnumerable entity, Uri requestUri)
    {
      var arrayOfObjects = entity.Cast<object>().ToArray();
      return new Collection
      {
        Member = arrayOfObjects
      };
    }
  }
}