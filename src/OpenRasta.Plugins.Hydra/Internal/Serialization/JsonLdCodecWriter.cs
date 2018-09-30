using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class FastUriGenerator
  {
    readonly IUriResolver _uris;
    Dictionary<Type, Func<object, string>> _generators;
    public FastUriGenerator(IMetaModelRepository repository, IUriResolver uris)
    {
      _uris = uris;
      _generators =
        (from model in repository.ResourceRegistrations
          let generators = model.Uris.Where(uri => uri.Properties.ContainsKey("compiled")).ToList()
          where generators.Count == 1
          select new {model.ResourceType, generator = (Func<object,string>)(generators[0].Properties["compiled"])})
        .ToDictionary(x => x.ResourceType, x => x.generator);
    }

    public string CreateUri<T>(Uri baseUri)
    {
      if (_generators.TryGetValue(typeof(T), out var generator))
      {
        return baseUri + generator(null).Substring(1);
        
      }

      return _uris.CreateUriFor(baseUri,typeof(T)).ToString();
    }
    public string CreateUri(object instance, Uri baseUri)
    {
      if (_generators.TryGetValue(instance.GetType(), out var generator))
      {
        return baseUri + generator(instance).Substring(1);
        
      }

      return _uris.CreateFrom(instance, baseUri).ToString();
    }
  }
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

      var func = _context.PipelineData.SelectedResource.ResourceModel.Hydra().SerializeFunc;

      if (func == null) throw new InvalidOperationException($"Hydra serialiser not found for object of type {entity?.GetType()}");


      return func(entity, new SerializationContext
      {
        BaseUri = BaseUri,
        UriGenerator = resource => _uris.CreateUri(resource,_context.ApplicationBaseUri)
      }, response.Stream);
    }
  }
}